using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using Squared.Util.Event;
using System.IO;
using System.Net;
using ShootBlues.Profile;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;
using Squared.Util;
using System.Threading;
using System.Drawing.Drawing2D;

namespace ShootBlues.Script {
    static class BitmapDataExtensions {
        public static unsafe UInt32* Ptr (this BitmapData bd, int x, int y) {
            return (UInt32*)((byte*)bd.Scan0 + (y * bd.Stride) + (x * 4));
        }
    }

    public class RemoteControl : ManagedScript {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll")]
        static extern bool GetClientRect (IntPtr hWnd, out RECT lpRect);
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow (IntPtr hwnd, IntPtr hDC, uint nFlags);

        const float FrameCaptureInterval = 1.0f / 15f;
        const long KeyframeQuality = 50;
        const int KeyframeInterval = 128;
        const long DeltaQuality = 40;
        const int ChangeThreshold = 20;
        const int BlockSize = 8;

        Bitmap FrameBuffer = null;
        long LastFrameTimestamp = 0;

        MemoryStream OutputFrameStream = new MemoryStream();
        Bitmap LastOutputFrame, OutputFrame, Mask;
        long FrameIndex = -12344, MaskIndex = -12344;

        object FrameLock = new object(), MaskLock = new object();

        ToolStripMenuItem CustomMenu;
        HttpListener HttpListener;

        IFuture HttpTaskFuture;

        public RemoteControl (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");

            /*
            CustomMenu = new ToolStripMenuItem("Remote Control");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureRemoteControl);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
             */
        }

        public override void Dispose () {
            base.Dispose();

            if (FrameBuffer != null)
                FrameBuffer.Dispose();
            HttpTaskFuture.Dispose();
            HttpListener.Stop();
            /*
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
             */
        }

        protected IEnumerator<object> HttpTask () {
            while (true) {
                var fContext = HttpListener.GetContextAsync();
                yield return fContext;

                yield return new RunAsBackground(RequestTask(fContext.Result));
            }
        }

        protected IEnumerator<object> RequestTask (HttpListenerContext context) {
            using (context.Response) {
                var path = context.Request.Url.AbsolutePath;

                IEnumerator<object> task;
                switch (path) {
                    case "/eve":
                    case "/eve/":
                        task = ServeStaticFile(context, "index.html", "text/html");
                        break;
                    case "/eve/viewport":
                        task = ServeWindowScreenshot(context);
                        break;
                    case "/eve/decode.js":
                        task = ServeStaticFile(context, "decode.js", "text/javascript");
                        break;
                    default:
                        task = ServeError(context, 404, "Invalid address");
                        break;
                }

                var fTask = Scheduler.Start(task, TaskExecutionPolicy.RunAsBackgroundTask);
                yield return fTask;

                if (fTask.Failed) {
                    try {
                        context.Response.StatusCode = 500;
                        context.Response.StatusDescription = "Internal server error";
                        context.Response.Close();
                    } catch (InvalidOperationException) {
                    }
                } else {
                    try {
                        context.Response.Close();
                    } catch (InvalidOperationException) {
                    }
                }
            }
        }

        protected Bitmap GetSizedBitmap (int width, int height, ref Bitmap bitmap, PixelFormat pf) {
            if ((bitmap == null) ||
                (bitmap.Width != width) ||
                (bitmap.Height != height)) {
                if (bitmap != null)
                    bitmap.Dispose();

                bitmap = new Bitmap(width, height, pf);
            }

            return bitmap;
        }

        protected Graphics GetGraphics (Bitmap bitmap) {
            var g = Graphics.FromImage(bitmap);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Bilinear;
            return g;
        }

        protected unsafe void GenerateImages (IntPtr hWnd, int width, int height, long frameIndex, Action<Stream, Bitmap, long> encoder) {
            RECT windowRect;
            IntPtr hDC = IntPtr.Zero;
            GetClientRect(hWnd, out windowRect);

            int windowWidth = windowRect.Right - windowRect.Left;
            int windowHeight = windowRect.Bottom - windowRect.Top;

            float scale = Math.Min(
                Math.Min(
                    width / (float)windowWidth, 
                    height / (float)windowHeight
                ), 1.0f
            );

            long oldFrameIndex = FrameIndex;
            Bitmap oldFrame, newFrame, frameBuffer, mask;
            frameBuffer = GetSizedBitmap(windowWidth, windowHeight, ref FrameBuffer, PixelFormat.Format32bppRgb);
            long now = Time.Ticks;
            long timeDelta = now - LastFrameTimestamp;

            if (timeDelta > (Time.SecondInTicks * FrameCaptureInterval)) {
                using (var g = GetGraphics(frameBuffer))
                try {
                    hDC = g.GetHdc();
                    PrintWindow(hWnd, hDC, 1);
                } finally {
                    g.ReleaseHdc(hDC);
                }
                LastFrameTimestamp = now;
            }           

            int outputWidth = (int)Math.Max(Math.Floor(frameBuffer.Width * scale), 1);
            int outputHeight = (int)Math.Max(Math.Floor(frameBuffer.Height * scale), 1);

            int maskWidth = (int)Math.Ceiling(outputWidth / (float)BlockSize);
            int maskHeight = (int)Math.Ceiling(outputHeight / (float)BlockSize);

            bool isKeyframe = ((frameIndex % KeyframeInterval) == 0) || 
                (oldFrameIndex != (frameIndex - 1)) ||
                (LastOutputFrame == null) || (LastOutputFrame.Width != outputWidth) ||
                (LastOutputFrame.Height != outputHeight);

            oldFrame = GetSizedBitmap(outputWidth, outputHeight, ref LastOutputFrame, PixelFormat.Format32bppRgb);
            newFrame = GetSizedBitmap(outputWidth, outputHeight, ref OutputFrame, PixelFormat.Format32bppRgb);
            mask = GetSizedBitmap(maskWidth, maskHeight, ref Mask, PixelFormat.Format32bppRgb);

            var outputRect = new Rectangle(0, 0, outputWidth, outputHeight);
            if (isKeyframe) {
                // Prior frame is gone, so send raw image
                lock (MaskLock)
                    using (var g = GetGraphics(mask))
                        g.Clear(Color.White);
                using (var g = GetGraphics(oldFrame)) {
                    if (scale == 1.0f)
                        g.DrawImageUnscaledAndClipped(frameBuffer, outputRect);
                    else
                        g.DrawImage(frameBuffer, outputRect, new Rectangle(0, 0, frameBuffer.Width, frameBuffer.Height), GraphicsUnit.Pixel);
                }
                using (var g = GetGraphics(newFrame))
                    g.DrawImageUnscaledAndClipped(oldFrame, outputRect);

                OutputFrameStream.Seek(0, SeekOrigin.Begin);
                OutputFrameStream.SetLength(0);

                encoder(OutputFrameStream, newFrame, KeyframeQuality);
            } else {
                Rectangle lockRect, maskLockRect;
                BitmapData bdOld, bdNew, bdMask;
                lock (MaskLock) {
                    // Send delta image
                    using (var g = GetGraphics(mask))
                        g.Clear(Color.FromArgb(0, 0, 0, 0));
                    using (var g = GetGraphics(newFrame)) {
                        if (scale == 1.0f)
                            g.DrawImageUnscaledAndClipped(frameBuffer, outputRect);
                        else
                            g.DrawImage(frameBuffer, outputRect, new Rectangle(0, 0, frameBuffer.Width, frameBuffer.Height), GraphicsUnit.Pixel);
                    }

                    lockRect = new Rectangle(0, 0, outputWidth, outputHeight);
                    maskLockRect = new Rectangle(0, 0, maskWidth, maskHeight);

                    bdOld = oldFrame.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                    bdNew = newFrame.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                    bdMask = mask.LockBits(maskLockRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                    for (int y = 0; y < outputHeight; y++) {
                        for (int x = 0; x < outputWidth; x++) {
                            var pOld = (byte*)bdOld.Ptr(x, y);
                            var pNew = (byte*)bdNew.Ptr(x, y);
                            var pMask = bdMask.Ptr(x / BlockSize, y / BlockSize);

                            var rDelta = Math.Abs(pNew[0] - pOld[0]) +
                                Math.Abs(pNew[1] - pOld[1]) +
                                Math.Abs(pNew[2] - pOld[2]);

                            if (rDelta > ChangeThreshold) {
                                *pMask = 0xFFFFFFFF;
                                x += BlockSize - (x % BlockSize) - 1;
                            }
                        }
                    }

                    mask.UnlockBits(bdMask);

                    MaskIndex = frameIndex;
                }

                bdMask = mask.LockBits(maskLockRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

                Rectangle rect = new Rectangle();
                for (int y = 0; y < maskHeight; y++, rect.Y += BlockSize) {
                    rect.X = 0;
                    rect.Height = Math.Min(BlockSize, bdNew.Height - rect.Y);

                    for (int x = 0; x < maskWidth; x++, rect.X += BlockSize) {
                        rect.Width = Math.Min(BlockSize, bdNew.Width - rect.X);

                        var pMask = bdMask.Ptr(x, y);
                        if (*pMask != 0) {
                            for (int cy = 0; cy < rect.Height; cy++) {
                                var pOld = bdOld.Ptr(rect.X, rect.Y + cy);
                                var pNew = bdNew.Ptr(rect.X, rect.Y + cy);
                                for (int cx = 0; cx < rect.Width; cx++, pOld++, pNew++)
                                    *pOld = *pNew;
                            }
                        } else {
                            for (int cy = 0; cy < rect.Height; cy++) {
                                var pNew = bdNew.Ptr(rect.X, rect.Y + cy);
                                for (int cx = 0; cx < rect.Width; cx++, pNew++)
                                    *pNew = 0;
                            }
                        }
                    }
                }

                OutputFrameStream.Seek(0, SeekOrigin.Begin);
                OutputFrameStream.SetLength(0);

                encoder(OutputFrameStream, newFrame, DeltaQuality);

                newFrame.UnlockBits(bdNew);
                oldFrame.UnlockBits(bdOld);
                mask.UnlockBits(bdMask);
            }

            if (isKeyframe)
                Console.WriteLine("Keyframe {0} bytes", OutputFrameStream.Length);
            else
                Console.WriteLine("Deltas {0} bytes", OutputFrameStream.Length);

            FrameIndex = frameIndex;
        }

        private ImageCodecInfo GetEncoder (ImageFormat format) {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }

        public IEnumerator<object> ServeWindowScreenshot (HttpListenerContext context) {
            if (Program.RunningProcesses.Count == 0) {
                yield return ServeError(context, 500, "No running EVE processes");
                yield break;
            }

            string type = context.Request.QueryString["t"];
            int width = int.Parse(context.Request.QueryString["w"]);
            int height = int.Parse(context.Request.QueryString["h"]);
            long frameIndex = long.Parse(context.Request.QueryString["i"]);

            var windowPtr = GetProcessWindow(Program.RunningProcesses.First());

            context.Response.AppendHeader("Cache-Control", "no-store, no-cache, private");

            var codec = GetEncoder(ImageFormat.Jpeg);
            var codecParams = new EncoderParameters(1);

            var fFrameStream = new Future<MemoryStream>();
            var fMask = new Future<Bitmap>();
            var jpegEncoder = (Action<Stream, Bitmap, long>)((s, b, q) => {
                codecParams.Param[0] = new EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, q
                );
                b.Save(s, codec, codecParams);
            });

            Action generateImages = () => {
                lock (FrameLock) {
                    if (frameIndex != FrameIndex)                        
                        GenerateImages(windowPtr, width, height, frameIndex, jpegEncoder);

                    fFrameStream.Complete(this.OutputFrameStream);
                    fMask.Complete(this.Mask);
                }
            };

            if (type == "mask") {
                long currentIndex;
                lock (MaskLock) {
                    currentIndex = MaskIndex;
                    if (MaskIndex == frameIndex)
                        fMask.Complete(this.Mask);
                }

                if (currentIndex != frameIndex)
                    yield return Future.RunInThread(generateImages);
            } else {
                long currentIndex;
                lock (MaskLock) {
                    currentIndex = FrameIndex;
                    if (FrameIndex == frameIndex)
                        fFrameStream.Complete(this.OutputFrameStream);
                }

                if (currentIndex != frameIndex)
                    yield return Future.RunInThread(generateImages);
            }

            var memStream = new MemoryStream();

            if (type == "mask") {
                context.Response.ContentType = "image/png";

                memStream.Seek(0, SeekOrigin.Begin);
                yield return Future.RunInThread(() => {
                    fMask.Result.Save(memStream, ImageFormat.Png);
                });
            } else {
                context.Response.ContentType = "image/jpeg";

                memStream = OutputFrameStream;
            }

            context.Response.ContentLength64 = memStream.Length;
            memStream.Seek(0, SeekOrigin.Begin);
            try {
                CopyStream(memStream, context.Response.OutputStream);
            } catch (HttpListenerException) {
            }
        }

        public static Stream GetResourceStream (string filename) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(
                String.Format("ShootBlues.Script.{0}", filename)
            );
        }

        private static void CopyStream (Stream input, Stream output) {
            byte[] buffer = new byte[4096];
            while (true) {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    return;
                output.Write(buffer, 0, read);
            }
        }

        public static IEnumerator<object> ServeStaticFile (HttpListenerContext context, string filename, string contentType) {
            context.Response.ContentType = contentType;

            using (var src = GetResourceStream(filename)) {
                var resp = context.Response.OutputStream;
                yield return Future.RunInThread(() => CopyStream(src, resp));
            }
        }

        public static IEnumerator<object> ServeError (HttpListenerContext context, int errorCode, string errorText) {
            context.Response.StatusCode = errorCode;
            context.Response.StatusDescription = errorText;
            context.Response.ContentType = "text/html";

            using (var resp = context.GetResponseWriter(Encoding.UTF8)) {
                yield return resp.WriteLine(String.Format(
                    "<html><body>{0}</body></html>", HttpUtility.HtmlEncode(errorText)
                ));
            }
        }

        protected IntPtr GetProcessWindow (ProcessInfo pi) {
            var profile = (EVE)Profile;
            return profile.ProcessWindows[pi.Process.Id];
        }

        public void ConfigureRemoteControl (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Remote Control"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            /*
            using (var configWindow = new RemoteControlConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }
             */

            HttpListener = new HttpListener();
            HttpListener.IgnoreWriteExceptions = true;
            HttpListener.Prefixes.Add("http://localhost:91/eve/");
            HttpListener.Prefixes.Add("http://hildr.luminance.org:91/eve/");
            HttpListener.Start();

            HttpTaskFuture = Scheduler.Start(
                HttpTask(), TaskExecutionPolicy.RunAsBackgroundTask
            );

            yield break;
        }
    }
}
