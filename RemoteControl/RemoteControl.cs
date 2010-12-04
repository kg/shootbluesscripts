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
using System.Web.Script.Serialization;

namespace ShootBlues.Script {
    static class BitmapDataExtensions {
        public static unsafe UInt32* Ptr (this BitmapData bd, int x, int y) {
            return (UInt32*)((byte*)bd.Scan0 + (y * bd.Stride) + (x * 4));
        }
    }

    class PairConverter : JavaScriptConverter {
        public override object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer) {
            var p = (Pair<int>)obj;
            return new Dictionary<string, object> {
                {"x", p.First},
                {"y", p.Second}
            };
        }

        public override IEnumerable<Type> SupportedTypes {
            get {
                yield return typeof(Pair<int>);
            }
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

        const float FrameCaptureInterval = 1.0f / 30f;
        const long KeyframeQuality = 60;
        const int KeyframeInterval = 512;
        const long DeltaQuality = 35;
        const int ChangeThreshold = 24;
        const int BlockSize = 16;
        const int MaxDeltaBlocks = 1536;

        MemoryStream DeltaStream = new MemoryStream();
        Bitmap FrameBuffer = null, DeltaFrame = null, DeltaBlocks = null;
        Pair<int>[] DeltaIndices = null;
        long LastFrameTimestamp = 0;
        long FrameIndex = -12344;

        object FrameLock = new object();

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
            if (DeltaFrame != null)
                DeltaFrame.Dispose();

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

                yield return new Start(
                    RequestTask(fContext.Result), 
                    TaskExecutionPolicy.RunAsBackgroundTask
                );
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
                    case "/eve/viewport/deltas":
                        task = ServeViewportDeltas(context);
                        break;
                    case "/eve/viewport/indices":
                        task = ServeViewportIndices(context);
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
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            return g;
        }

        protected unsafe void GenerateDeltas (IntPtr hWnd, long frameIndex) {
            RECT windowRect;
            IntPtr hDC = IntPtr.Zero;
            GetClientRect(hWnd, out windowRect);

            int windowWidth = windowRect.Right - windowRect.Left;
            int windowHeight = windowRect.Bottom - windowRect.Top;

            if ((windowWidth == 0) || (windowHeight == 0)) {
                if ((DeltaIndices == null) || (DeltaIndices.Length != 0))
                    DeltaIndices = new Pair<int>[0];

                DeltaStream.Seek(0, SeekOrigin.Begin);
                DeltaStream.SetLength(0);

                FrameIndex = frameIndex;
                return;
            }

            long oldFrameIndex = FrameIndex;
            bool[] mask;
            Bitmap deltaFrame, blocks, frameBuffer;
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

            int maskWidth = (int)Math.Ceiling(windowWidth / (float)BlockSize);
            int maskHeight = (int)Math.Ceiling(windowHeight / (float)BlockSize);

            bool isKeyframe = ((frameIndex % KeyframeInterval) == 0) || 
                (oldFrameIndex != (frameIndex - 1)) ||
                (DeltaFrame == null) || (DeltaFrame.Width != windowWidth) ||
                (DeltaFrame.Height != windowHeight);

            var codec = GetEncoder(ImageFormat.Jpeg);
            var codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality, isKeyframe ? KeyframeQuality : DeltaQuality
            );

            deltaFrame = GetSizedBitmap(windowWidth, windowHeight, ref DeltaFrame, PixelFormat.Format32bppRgb);
            mask = new bool[maskWidth * maskHeight];

            var deltaRect = new Rectangle(0, 0, windowWidth, windowHeight);
            while (!isKeyframe) {
                BitmapData bdOld, bdNew;

                bdOld = deltaFrame.LockBits(deltaRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                bdNew = frameBuffer.LockBits(deltaRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                int deltaBlockCount = 0;

                fixed (bool* pMask = mask)
                for (int y = 0; y < windowHeight; y++) {
                    for (int x = 0; x < windowWidth; x++) {
                        int maskIndex = (y / BlockSize) * maskWidth + (x / BlockSize);
                        if (pMask[maskIndex]) {
                            x += BlockSize - (x % BlockSize) - 1;
                            continue;
                        }

                        var pOld = (byte*)bdOld.Ptr(x, y);
                        var pNew = (byte*)bdNew.Ptr(x, y);

                        var rDelta = Math.Abs(pNew[0] - pOld[0]) +
                            Math.Abs(pNew[1] - pOld[1]) +
                            Math.Abs(pNew[2] - pOld[2]);

                        if (rDelta > ChangeThreshold) {
                            pMask[maskIndex] = true;
                            deltaBlockCount += 1;
                            x += BlockSize - (x % BlockSize) - 1;
                        }
                    }
                }

                deltaFrame.UnlockBits(bdOld);
                frameBuffer.UnlockBits(bdNew);

                // Hack :(
                if (deltaBlockCount == 0) {
                    break;
                } else if (deltaBlockCount >= MaxDeltaBlocks) {
                    isKeyframe = true;
                    break;
                }

                blocks = GetSizedBitmap(BlockSize * deltaBlockCount, BlockSize, ref DeltaBlocks, PixelFormat.Format32bppRgb);
                if ((DeltaIndices == null) || (DeltaIndices.Length != deltaBlockCount + 1))
                    DeltaIndices = new Pair<int>[deltaBlockCount + 1];

                Rectangle rect = new Rectangle();
                fixed (bool* pMask = mask)
                using (var gblocks = GetGraphics(blocks))
                using (var gdelta = GetGraphics(deltaFrame)) {
                    var idx = new Pair<int>(windowWidth, windowHeight);
                    DeltaIndices[0] = idx;

                    gblocks.Clear(Color.Black);

                    for (int i = 0, j = 0; i < mask.Length; i++) {
                        if (!pMask[i])
                            continue;

                        idx.First = rect.X = (i % maskWidth) * BlockSize;
                        idx.Second = rect.Y = (i / maskWidth) * BlockSize;
                        DeltaIndices[j + 1] = idx;
                        rect.Width = BlockSize;
                        rect.Height = BlockSize;
                        gblocks.DrawImage(frameBuffer, j * BlockSize, 0, rect, GraphicsUnit.Pixel);
                        gdelta.DrawImage(frameBuffer, rect, rect, GraphicsUnit.Pixel);

                        j += 1;
                    }
                }

                DeltaStream.Seek(0, SeekOrigin.Begin);
                DeltaStream.SetLength(0);
                blocks.Save(DeltaStream, codec, codecParams);
                Console.WriteLine("Deltas {0:0000000} byte(s)", DeltaStream.Length);

                FrameIndex = frameIndex;

                return;
            }

            if (isKeyframe) {
                using (var g = GetGraphics(deltaFrame))
                    g.DrawImageUnscaledAndClipped(frameBuffer, deltaRect);

                if ((DeltaIndices == null) || (DeltaIndices.Length != 1)) {
                    DeltaIndices = new Pair<int>[1];
                    DeltaIndices[0] = new Pair<int>(windowWidth, windowHeight);
                }

                DeltaStream.Seek(0, SeekOrigin.Begin);
                DeltaStream.SetLength(0);
                deltaFrame.Save(DeltaStream, codec, codecParams);

                Console.WriteLine("Keyframe {0:0000000} byte(s)", DeltaStream.Length);
            } else {
                if ((DeltaIndices == null) || (DeltaIndices.Length != 0))
                    DeltaIndices = new Pair<int>[0];

                DeltaStream.Seek(0, SeekOrigin.Begin);
                DeltaStream.SetLength(0);
            }

            FrameIndex = frameIndex;
        }

        private ImageCodecInfo GetEncoder (ImageFormat format) {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }

        public IEnumerator<object> ServeViewportDeltas (HttpListenerContext context) {
            if (Program.RunningProcesses.Count == 0) {
                yield return ServeError(context, 500, "No running EVE processes");
                yield break;
            }

            var windowPtr = GetProcessWindow(Program.RunningProcesses.First());
            long frameIndex = long.Parse(context.Request.QueryString["i"]);

            context.Response.AppendHeader("Cache-Control", "no-store, no-cache, private");
            context.Response.ContentType = "image/jpeg";

            yield return Future.RunInThread(() => {
                lock (FrameLock) {
                    if (frameIndex != FrameIndex)
                        GenerateDeltas(windowPtr, frameIndex);

                    DeltaStream.Seek(0, SeekOrigin.Begin);
                    try {
                        context.Response.ContentLength64 = DeltaStream.Length;
                        CopyStream(DeltaStream, context.Response.OutputStream);
                    } catch (HttpListenerException) {
                    }
                }
            });
        }

        public IEnumerator<object> ServeViewportIndices (HttpListenerContext context) {
            if (Program.RunningProcesses.Count == 0) {
                yield return ServeError(context, 500, "No running EVE processes");
                yield break;
            }

            var windowPtr = GetProcessWindow(Program.RunningProcesses.First());
            long frameIndex = long.Parse(context.Request.QueryString["i"]);

            context.Response.AppendHeader("Cache-Control", "no-store, no-cache, private");
            context.Response.ContentType = "application/json";

            var ser = new JavaScriptSerializer();
            var converter = new PairConverter();
            ser.RegisterConverters(new[] { converter });

            var fStr = Future.RunInThread(() => {
                lock (FrameLock) {
                    if (FrameIndex != frameIndex)
                        GenerateDeltas(windowPtr, frameIndex);

                    return ser.Serialize(DeltaIndices);
                }
            });
            yield return fStr;

            var enc = Encoding.UTF8;
            context.Response.ContentLength64 = enc.GetByteCount(fStr.Result);
            using (var writer = context.GetResponseWriter(enc))
                yield return writer.Write(fStr.Result);
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
