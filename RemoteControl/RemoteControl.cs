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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;

namespace ShootBlues.Script {
    static class BitmapDataExtensions {
        public static unsafe byte* Ptr (this BitmapData bd, int x, int y) {
            return (byte*)bd.Scan0 + (y * bd.Stride) + (x * 3);
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
        [Flags]
        public enum MouseButtons : uint {
            None = 0x00,
            Left = 0x01,
            Middle = 0x10,
            Right = 0x02
        }

        public class ProcessState : IDisposable {
            public readonly IntPtr hWnd;
            public readonly KernelFunctionDisabler FuncDisabler;

            public MouseButtons HeldMouseButtons = MouseButtons.None;
            public Bitmap FrameBuffer = null, DeltaFrame = null;
            public object FrameLock = new object();
            public MemoryStream DeltaStream = new MemoryStream();
            public Pair<int>[] DeltaIndices = null;
            public long FrameIndex = -12345;
            public long LastFrameTimestamp = 0;

            public ProcessState (ProcessInfo pi) {
                FuncDisabler = new KernelFunctionDisabler(pi);
                var profile = (EVE)Program.Profile;
                hWnd = profile.ProcessWindows[pi.Process.Id];

                FuncDisabler.DisableFunction("user32.dll", "SetCursorPos");
                FuncDisabler.DisableFunction("user32.dll", "ClipCursor");
            }

            public void Dispose () {
                lock (FrameLock) {
                    DeltaStream.SetLength(0);
                    if (FrameBuffer != null) {
                        FrameBuffer.Dispose();
                        FrameBuffer = null;
                    }
                    FuncDisabler.Dispose();
                    DeltaIndices = null;
                    FrameIndex = -12345;
                    LastFrameTimestamp = 0;
                }
            }
        }

        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_MBUTTONUP = 0x0208;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;

        const int BlockSize = 16;
        const int KeyframeInterval = 512;

        Regex RequestRegex = new Regex(
            @"/eve/(?'pid'[0-9]*)(?'rest'(/)(.*))", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline
        );

        Dictionary<string, object> CurrentPrefs = null;
        Dictionary<int, ProcessState> States = new Dictionary<int, ProcessState>();

        ToolStripMenuItem CustomMenu;
        HttpListener HttpListener;

        IFuture HttpTaskFuture, GCTaskFuture;

        public RemoteControl (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");

            CustomMenu = new ToolStripMenuItem("Remote Control");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureRemoteControl);
            CustomMenu.DropDownItems.Add("Open In Browser", null, OpenInBrowser);
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();

            HttpTaskFuture.Dispose();
            GCTaskFuture.Dispose();

            foreach (var ps in States.Values)
                ps.Dispose();
            States.Clear();

            HttpListener.Stop();
            HttpListener.Close();

            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        protected T GetPref<T> (string prefName, T defaultValue) {
            if (CurrentPrefs == null)
                return defaultValue;

            object result;
            if (CurrentPrefs.TryGetValue(prefName, out result))
                return (T)Convert.ChangeType(result, typeof(T));
            else 
                return defaultValue;
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

        protected IEnumerator<object> GCTask () {
            var sleep = new Sleep(60.0);

            while (true) {
                yield return sleep;

                var runningPids = new HashSet<int>((from pi in Program.RunningProcesses select pi.Process.Id).ToArray());
                var pids = States.Keys.ToArray();
                foreach (var pid in pids) {
                    if (!runningPids.Contains(pid)) {
                        States[pid].Dispose();
                        States.Remove(pid);
                    }
                }
            }
        }

        protected IEnumerator<object> RequestTask (HttpListenerContext context) {
            using (context.Response) {
                IEnumerator<object> task = null;
                var path = context.Request.Url.AbsolutePath;

                Match m = RequestRegex.Match(path);
                if ((m != null) && m.Success) {
                    var pid = int.Parse(m.Groups["pid"].Value);
                    path = m.Groups["rest"].Value;

                    ProcessInfo process = null;
                    try {
                        process = (from pi in Program.RunningProcesses
                                   where pi.Process.Id == pid
                                   select pi).First();
                    } catch {
                        task = ServeError(context, 404, "No process running with that ID");
                    }

                    if (task == null)
                    switch (path) {
                        case "/":
                        case "/index":
                            task = ServeStaticFile(context, "process_index.html", "text/html");
                        break;
                        case "/viewport/deltas":
                            task = ServeViewportDeltas(context, process);
                        break;
                        case "/viewport/indices":
                            task = ServeViewportIndices(context, process);
                        break;
                        case "/event/mousemove":
                            task = ProcessMouseEvent(context, process, "move");
                        break;
                        case "/event/mousedown":
                            task = ProcessMouseEvent(context, process, "down");
                        break;
                        case "/event/mouseup":
                            task = ProcessMouseEvent(context, process, "up");
                        break;
                        case "/event/keypress":
                            task = ProcessKeyEvent(context, process);
                        break;
                    }
                } else {
                    switch (path) {
                        case "/eve/":
                        case "/eve/index":
                            task = ServeProcessList(context);
                        break;
                    }
                }

                if (task == null)
                    task = ServeError(context, 404, "Invalid address");

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

        protected unsafe void GenerateDeltas (ProcessState ps, long frameIndex) {
            Rect windowRect;
            IntPtr hDC = IntPtr.Zero;
            Win32.GetClientRect(ps.hWnd, out windowRect);

            int windowWidth = windowRect.Right - windowRect.Left;
            int windowHeight = windowRect.Bottom - windowRect.Top;

            if ((windowWidth == 0) || (windowHeight == 0)) {
                if ((ps.DeltaIndices == null) || (ps.DeltaIndices.Length != 0))
                    ps.DeltaIndices = new Pair<int>[0];

                ps.DeltaStream.Seek(0, SeekOrigin.Begin);
                ps.DeltaStream.SetLength(0);

                ps.FrameIndex = frameIndex;
                return;
            }

            long oldFrameIndex = ps.FrameIndex;
            bool[] mask;
            Bitmap deltaFrame, blocks = null, frameBuffer;
            frameBuffer = GetSizedBitmap(windowWidth, windowHeight, ref ps.FrameBuffer, PixelFormat.Format24bppRgb);
            long now = Time.Ticks;
            long timeDelta = now - ps.LastFrameTimestamp;

            float frameCaptureInterval = 1.0f / GetPref("MaxFrameRate", 30);

            if (timeDelta > (Time.SecondInTicks * frameCaptureInterval)) {
                using (var g = GetGraphics(frameBuffer))
                try {
                    hDC = g.GetHdc();
                    Win32.PrintWindow(ps.hWnd, hDC, 1);
                } finally {
                    g.ReleaseHdc(hDC);
                }
                ps.LastFrameTimestamp = now;
            }

            int keyframeQuality = GetPref("KeyframeQuality", 70);
            int deltaQuality = GetPref("UpdateQuality", 40);
            int changeThreshold = Math.Max(4, 40 - ((keyframeQuality + deltaQuality) / 4));

            int maskWidth = (int)Math.Ceiling(windowWidth / (float)BlockSize);
            int maskHeight = (int)Math.Ceiling(windowHeight / (float)BlockSize);

            bool isKeyframe = ((frameIndex % KeyframeInterval) == 0) || 
                (oldFrameIndex != (frameIndex - 1)) ||
                (ps.DeltaFrame == null) || (ps.DeltaFrame.Width != windowWidth) ||
                (ps.DeltaFrame.Height != windowHeight);

            var codec = GetEncoder(ImageFormat.Jpeg);
            var codecParams = new EncoderParameters(1);
            codecParams.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality, isKeyframe ? keyframeQuality : deltaQuality
            );

            deltaFrame = GetSizedBitmap(windowWidth, windowHeight, ref ps.DeltaFrame, PixelFormat.Format24bppRgb);
            mask = new bool[maskWidth * maskHeight];

            var deltaRect = new Rectangle(0, 0, windowWidth, windowHeight);
            while (!isKeyframe) {
                BitmapData bdOld, bdNew;

                bdOld = deltaFrame.LockBits(deltaRect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                bdNew = frameBuffer.LockBits(deltaRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                int deltaBlockCount = 0;
                int d0, d1, d2, rDelta;

                try {
                    fixed (bool* pMask = mask)
                    for (int y = 0; y < windowHeight; y++) {
                        for (int x = 0; x < windowWidth; x++) {
                            int maskIndex = (y / BlockSize) * maskWidth + (x / BlockSize);
                            if (pMask[maskIndex]) {
                                x += BlockSize - (x % BlockSize) - 1;
                                continue;
                            }

                            var pOld = bdOld.Ptr(x, y);
                            var pNew = bdNew.Ptr(x, y);

                            d0 = pNew[0] - pOld[0];
                            d1 = pNew[1] - pOld[1];
                            d2 = pNew[2] - pOld[2];
                            rDelta = ((d0 ^ (d0 >> 31)) - (d0 >> 31)) +
                                ((d1 ^ (d1 >> 31)) - (d1 >> 31)) +
                                ((d2 ^ (d2 >> 31)) - (d2 >> 31));

                            if (rDelta > changeThreshold) {
                                pMask[maskIndex] = true;
                                deltaBlockCount += 1;
                                x += BlockSize - (x % BlockSize) - 1;
                            }
                        }
                    }

                    int maxDeltaBlocks = GetPref("KeyframeThreshold", 1500);

                    // Hack :(
                    if (deltaBlockCount == 0) {
                        break;
                    } else if (deltaBlockCount >= maxDeltaBlocks) {
                        isKeyframe = true;
                        break;
                    }

                    blocks = new Bitmap(BlockSize * deltaBlockCount, BlockSize, PixelFormat.Format24bppRgb);
                    if ((ps.DeltaIndices == null) || (ps.DeltaIndices.Length != deltaBlockCount + 1))
                        ps.DeltaIndices = new Pair<int>[deltaBlockCount + 1];

                    var blockRect = new Rectangle(0, 0, blocks.Width, blocks.Height);
                    var bdBlocks = blocks.LockBits(blockRect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                    fixed (bool* pMask = mask) {
                        var idx = new Pair<int>(windowWidth, windowHeight);
                        ps.DeltaIndices[0] = idx;

                        for (int i = 0, j = 0; i < mask.Length; i++) {
                            if (!pMask[i])
                                continue;

                            int blockX = j * BlockSize;
                            int copyX = idx.First = (i % maskWidth) * BlockSize;
                            int copyY = idx.Second = (i / maskWidth) * BlockSize;
                            ps.DeltaIndices[j + 1] = idx;

                            int copyWidth = Math.Min(windowWidth - copyX, BlockSize) * 3;
                            int copyHeight = Math.Min(windowHeight - copyY, BlockSize);

                            for (int y = 0; y < copyHeight; y++) {
                                var pSrc = bdNew.Ptr(copyX, copyY + y);
                                var pDest1 = bdBlocks.Ptr(blockX, y);
                                var pDest2 = bdOld.Ptr(copyX, copyY + y);

                                for (int x = 0; x < copyWidth; x++, pSrc++, pDest1++, pDest2++)
                                    *pDest1 = *pDest2 = *pSrc;
                            }

                            j += 1;
                        }
                    }

                    blocks.UnlockBits(bdBlocks);

                    ps.DeltaStream.Seek(0, SeekOrigin.Begin);
                    ps.DeltaStream.SetLength(0);
                    blocks.Save(ps.DeltaStream, codec, codecParams);
                } finally {
                    if (blocks != null)
                        blocks.Dispose();

                    deltaFrame.UnlockBits(bdOld);
                    frameBuffer.UnlockBits(bdNew);
                }

                ps.FrameIndex = frameIndex;

                return;
            }

            if (isKeyframe) {
                var bdNew = frameBuffer.LockBits(deltaRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var bdOld = deltaFrame.LockBits(deltaRect, ImageLockMode.WriteOnly | ImageLockMode.UserInputBuffer, PixelFormat.Format24bppRgb, bdNew);
                deltaFrame.UnlockBits(bdOld);
                frameBuffer.UnlockBits(bdNew);

                if ((ps.DeltaIndices == null) || (ps.DeltaIndices.Length != 1)) {
                    ps.DeltaIndices = new Pair<int>[1];
                    ps.DeltaIndices[0] = new Pair<int>(windowWidth, windowHeight);
                }

                ps.DeltaStream.Seek(0, SeekOrigin.Begin);
                ps.DeltaStream.SetLength(0);
                deltaFrame.Save(ps.DeltaStream, codec, codecParams);
            } else {
                if ((ps.DeltaIndices == null) || (ps.DeltaIndices.Length != 0))
                    ps.DeltaIndices = new Pair<int>[0];

                ps.DeltaStream.Seek(0, SeekOrigin.Begin);
                ps.DeltaStream.SetLength(0);
            }

            ps.FrameIndex = frameIndex;
        }

        private ImageCodecInfo GetEncoder (ImageFormat format) {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }

        protected ProcessState GetState (ProcessInfo process) {
            ProcessState result;
            if (!States.TryGetValue(process.Process.Id, out result)) {
                result = new ProcessState(process);
                States[process.Process.Id] = result;
            }

            return result;
        }

        public IEnumerator<object> ServeViewportDeltas (HttpListenerContext context, ProcessInfo process) {
            var ps = GetState(process);
            long frameIndex = long.Parse(context.Request.QueryString["i"]);

            context.Response.AppendHeader("Cache-Control", "no-store, no-cache, private");
            context.Response.ContentType = "image/jpeg";

            yield return Future.RunInThread(() => {
                lock (ps.FrameLock) {
                    if (frameIndex != ps.FrameIndex)
                        GenerateDeltas(ps, frameIndex);

                    ps.DeltaStream.Seek(0, SeekOrigin.Begin);
                    try {
                        context.Response.ContentLength64 = ps.DeltaStream.Length;
                        ps.DeltaStream.WriteTo(context.Response.OutputStream);
                    } catch (HttpListenerException) {
                    }
                }
            });
        }

        public IEnumerator<object> ServeViewportIndices (HttpListenerContext context, ProcessInfo process) {
            var ps = GetState(process);
            long frameIndex = long.Parse(context.Request.QueryString["i"]);

            context.Response.AppendHeader("Cache-Control", "no-store, no-cache, private");
            context.Response.ContentType = "application/json";

            var ser = new JavaScriptSerializer();
            var converter = new PairConverter();
            ser.RegisterConverters(new[] { converter });

            var fStr = Future.RunInThread(() => {
                lock (ps.FrameLock) {
                    if (ps.FrameIndex != frameIndex)
                        GenerateDeltas(ps, frameIndex);

                    return ser.Serialize(ps.DeltaIndices);
                }
            });
            yield return fStr;

            var enc = Encoding.UTF8;
            context.Response.ContentLength64 = enc.GetByteCount(fStr.Result);
            using (var writer = context.GetResponseWriter(enc))
                yield return writer.Write(fStr.Result);
        }

        public IEnumerator<object> ProcessMouseEvent (HttpListenerContext context, ProcessInfo process, string eventType) {
            if (context.Request.HttpMethod != "POST") {
                yield return ServeError(context, 500, "Events must be sent via POST");
                yield break;
            }

            var ps = GetState(process);

            NameValueCollection requestBody = null;
            yield return Future.RunInThread(() => context.ParseRequestBody())
                .Bind(() => requestBody);

            if (requestBody == null) {
                yield return ServeError(context, 500, "Events require a request body");
                yield break;
            }
            
            short x = short.Parse(requestBody["x"]);
            short y = short.Parse(requestBody["y"]);

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(x);
            bw.Write(y);

            int msg = 0;
            switch (eventType) {
                case "move":
                    msg = WM_MOUSEMOVE;
                    break;
                case "down": {
                    var button = (MouseButtons)Enum.Parse(typeof(MouseButtons), requestBody["button"]);
                    switch (button) {
                        case MouseButtons.Left:
                            msg = WM_LBUTTONDOWN;
                        break;
                        case MouseButtons.Middle:
                            msg = WM_MBUTTONDOWN;
                        break;
                        case MouseButtons.Right:
                            msg = WM_RBUTTONDOWN;
                        break;
                    }
                    ps.HeldMouseButtons |= button;
                    break;
                }
                case "up": {
                    var button = (MouseButtons)Enum.Parse(typeof(MouseButtons), requestBody["button"]);
                    switch (button) {
                        case MouseButtons.Left:
                            msg = WM_LBUTTONUP;
                        break;
                        case MouseButtons.Middle:
                            msg = WM_MBUTTONUP;
                        break;
                        case MouseButtons.Right:
                            msg = WM_RBUTTONUP;
                        break;
                    }
                    ps.HeldMouseButtons = ps.HeldMouseButtons & ~button;
                    break;
                }
                default:
                    yield return ServeError(context, 500, "Invalid event type");
                    yield break;
            }

            if (msg == 0)
                throw new Exception();

            var lParam = new IntPtr(BitConverter.ToInt32(ms.ToArray(), 0));
            var wParam = new IntPtr((uint)ps.HeldMouseButtons);

            yield return Future.RunInThread(
                () => Win32.SendMessage(ps.hWnd, msg, wParam, lParam)
            );

            context.Response.ContentType = "application/json";
            using (var resp = context.GetResponseWriter(Encoding.UTF8))
                yield return resp.WriteLine(@"{""success"": true}");
        }

        public IEnumerator<object> ProcessKeyEvent (HttpListenerContext context, ProcessInfo process) {
            if (context.Request.HttpMethod != "POST") {
                yield return ServeError(context, 500, "Events must be sent via POST");
                yield break;
            }

            var ps = GetState(process);

            NameValueCollection requestBody = null;
            yield return Future.RunInThread(() => context.ParseRequestBody())
                .Bind(() => requestBody);

            if (requestBody == null) {
                yield return ServeError(context, 500, "Events require a request body");
                yield break;
            }

            short charCode = short.Parse(requestBody["char"]);

            short virtCode = Win32.VkKeyScan(charCode);
            byte shiftState = (byte)((virtCode >> 8) & 0xFF);
            virtCode = (short)(virtCode & 0xFF);

            var bits = new BitArray(32);
            var arr = new int[1];

            bits[0] = true;
            bits[30] = false;
            bits[31] = false;
            bits.CopyTo(arr, 0);
            var lParam = new IntPtr(arr[0]);
            var wParam = new IntPtr((int)virtCode);

            // Yeah, I know, I know... Jesus Christ. There must be a better way to do this?
            yield return Future.RunInThread(() => {
                ps.FuncDisabler.DisableFunction("user32", "TranslateMessage");
                try {
                    Win32.SendMessage(ps.hWnd, WM_KEYDOWN, wParam, lParam);
                } finally {
                    ps.FuncDisabler.EnableFunction("user32", "TranslateMessage");
                }
            });

            wParam = new IntPtr((int)charCode);

            Win32.PostMessage(ps.hWnd, WM_CHAR, wParam, lParam);

            bits[30] = true;
            bits[31] = true;
            bits.CopyTo(arr, 0);
            lParam = new IntPtr(arr[0]);
            wParam = new IntPtr((int)virtCode);

            Win32.PostMessage(ps.hWnd, WM_KEYUP, wParam, lParam);

            context.Response.ContentType = "application/json";
            using (var resp = context.GetResponseWriter(Encoding.UTF8))
                yield return resp.WriteLine(@"{""success"": true}");
        }

        public Stream GetResourceStream (string filename) {
            if (Debugger.IsAttached) {
                var myAssembly = this.GetType().Assembly;
                var myAssemblyPath = Path.GetFullPath(Path.GetDirectoryName(myAssembly.Location)).ToLowerInvariant();
                var mySourcePath = Path.GetFullPath(myAssemblyPath.Replace(
                    @"\shootblues\bin", String.Format(
                        @"\shootbluesscripts\{0}", Name.NameWithoutExtension.ToLowerInvariant()
                        .Replace(".script", "")
                    )
                ));

                var searchPath = Path.Combine(mySourcePath, filename);
                if (File.Exists(searchPath))
                    return File.OpenRead(searchPath);
            }

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

        public static IEnumerator<object> ServeProcessList (HttpListenerContext context) {
            context.Response.ContentType = "text/html";

            using (var resp = context.GetResponseWriter(Encoding.UTF8)) {
                yield return resp.WriteLines(
                    "<html><head><title>EVE Remote Control</title></head>",
                    "<body><h1>EVE Remote Control</h1><ul>"
                );

                foreach (var pi in Program.RunningProcesses)
                    yield return resp.WriteLine(
                        String.Format(@"<li><a href=""{0}/"">{1}</a></li>", pi.Process.Id, HttpUtility.HtmlEncode(pi.ToString()))
                    );

                yield return resp.WriteLine(
                    "</ul></body></html>"
                );
            }
        }

        public IEnumerator<object> ServeStaticFile (HttpListenerContext context, string filename, string contentType) {
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
                    "<html><head><title>Error {0}</title></head><body>{1}</body></html>", errorCode, HttpUtility.HtmlEncode(errorText)
                ));
            }
        }

        public void ConfigureRemoteControl (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Remote Control"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public void OpenInBrowser (object sender, EventArgs args) {
            Process.Start(
                String.Format("http://localhost:{0}/eve/", GetPref("ServerPort", 80))
            );
        }

        protected override IEnumerator<object> OnPreferencesChanged (Squared.Util.Event.EventInfo evt, string[] prefNames) {
            var oldPort = GetPref("ServerPort", 80);

            yield return Preferences.GetAll().Bind(() => CurrentPrefs);

            if (GetPref("ServerPort", 80) != oldPort)
                yield return ResetListener();
        }

        protected IEnumerator<object> ResetListener () {
            if (HttpTaskFuture != null) {
                HttpTaskFuture.Dispose();
            }
            if (HttpListener != null) {
                HttpListener.Stop();
                HttpListener.Close();
            }

            HttpListener = new HttpListener();
            HttpListener.IgnoreWriteExceptions = true;
            HttpListener.Prefixes.Add(String.Format("http://*:{0}/eve/", GetPref("ServerPort", 80)));
            HttpListener.Start();

            HttpTaskFuture = Scheduler.Start(
                HttpTask(), TaskExecutionPolicy.RunAsBackgroundTask
            );

            yield return Future.RunInThread(() => {
                foreach (var s in States.Values) {
                    lock (s.FrameLock) {
                        s.FrameIndex = -1;
                        s.LastFrameTimestamp = 0;
                    }
                }
            });
        }

        public override IEnumerator<object> Initialize () {
            using (var configWindow = new RemoteControlConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }

            yield return Preferences.GetAll().Bind(() => CurrentPrefs);

            yield return ResetListener();

            GCTaskFuture = Scheduler.Start(
                GCTask(), TaskExecutionPolicy.RunAsBackgroundTask
            );

            yield break;
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new RemoteControlConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Remote Control", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Remote Control");
            yield break;
        }
    }
}
