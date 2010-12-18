using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using Squared.Task;
using Squared.Util;
using System.Media;

namespace ShootBlues.Script {
    public class Common : ManagedScript {
        public struct MessageData {
            public readonly ProcessInfo Source;
            public readonly Dictionary<string, object> Data;

            public MessageData (ProcessInfo source, Dictionary<string, object> data) {
                Source = source;
                Data = data;
            }

            public string Name {
                get {
                    object result;
                    Data.TryGetValue("__name__", out result);
                    return result as string;
                }
            }
        }

        protected LogWindow LogWindowInstance = null;
        protected PythonExplorer PythonExplorerInstance = null;
        protected Dictionary<int, BlockingQueue<MessageData>> MessageQueues = new Dictionary<int, BlockingQueue<MessageData>>();

        public List<string> Log = new List<string>();

        ToolStripMenuItem CustomMenu;

        public Common (ScriptName name)
            : base(name) {
            AddDependency("common.py");
            AddDependency("common.service.py");
            AddDependency("common.messaging.py");
            AddDependency("common.eve.py");
            AddDependency("common.eve.logger.py");
            AddDependency("common.eve.state.py");
            AddDependency("common.eve.charmonitor.py");
            AddDependency("pythonexplorer.py");

            CustomMenu = new ToolStripMenuItem("Common");
            CustomMenu.DropDown.Items.Add(
                new ToolStripMenuItem("Show Log", null, (s, e) => {
                    Program.Scheduler.Start(
                        ShowLogWindow(), Squared.Task.TaskExecutionPolicy.RunAsBackgroundTask
                    );
                })
            );
            CustomMenu.DropDown.Items.Add(
                new ToolStripMenuItem("Clear Log", null, (s, e) => {
                    LogClear();
                })
            );
            CustomMenu.DropDown.Items.Add(new ToolStripSeparator());
            CustomMenu.DropDown.Items.Add(
                new ToolStripMenuItem("Python Explorer", null, (s, e) => {
                    Program.Scheduler.Start(
                        ShowPythonExplorer(), Squared.Task.TaskExecutionPolicy.RunAsBackgroundTask
                    );
                })
            );
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public IEnumerator<object> ShowPythonExplorer () {
            if (PythonExplorerInstance != null) {
                PythonExplorerInstance.Activate();
                PythonExplorerInstance.Focus();
                yield break;
            }

            using (PythonExplorerInstance = new PythonExplorer(Program.Scheduler, this))
                yield return PythonExplorerInstance.Show();

            PythonExplorerInstance = null;
        }

        public IEnumerator<object> ShowLogWindow () {
            if (LogWindowInstance != null) {
                LogWindowInstance.Activate();
                LogWindowInstance.Focus();
                yield break;
            }

            using (LogWindowInstance = new LogWindow(Program.Scheduler)) {
                LogWindowInstance.SetText(
                    String.Join(Environment.NewLine, Log.ToArray())
                );
                yield return LogWindowInstance.Show();
            }

            LogWindowInstance = null;
        }

        public void ShowError (ProcessInfo process, string errorText) {
            Program.ShowErrorMessage(errorText, process);
        }

        public static IEnumerator<object> CreateNamedChannel (ProcessInfo process, string name) {
            var channel = process.GetNamedChannel(name);

            yield return Program.CallFunction(process, "common", "_initChannel", name, channel.ChannelID);
        }

        override public IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return CreateNamedChannel(process, "log");
            yield return CreateNamedChannel(process, "remotecall");
            yield return CreateNamedChannel(process, "messages");

            MessageQueues[process.Process.Id] = new BlockingQueue<MessageData>();

            process.Start(LogTask(process));
            process.Start(RemoteCallTask(process));
            process.Start(MessageListenerTask(process));
            process.Start(MessageDispatcherTask(process));

            yield return Program.CallFunction(process, "common", "initialize", process.Process.Id);
        }

        public override IEnumerator<object> UnloadFrom (ProcessInfo process) {
            var pid = process.Process.Id;

            if (MessageQueues.ContainsKey(pid))
                MessageQueues.Remove(pid);

            yield break;
        }

        private IEnumerator<object> MessageListenerTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("messages");
            var serializer = new JavaScriptSerializer();

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                MessageData mdata;
                try {
                    var json = fNext.Result.DecodeUTF8Z();
                    mdata = new MessageData(
                        process,
                        serializer.Deserialize<Dictionary<string, object>>(json)
                    );
                } catch (Exception ex) {
                    LogPrint(process, String.Format("Failed to parse message payload: {0}", ex));
                    continue;
                }

                foreach (var queue in MessageQueues.Values)
                    queue.Enqueue(mdata);

                EventBus.Broadcast(process, "Message", mdata);
                EventBus.Broadcast(process, mdata.Name, mdata);
            }
        }

        private IEnumerator<object> MessageDispatcherTask (ProcessInfo process) {
            var queue = MessageQueues[process.Process.Id];
            var sleep = new Sleep(0.01);

            while (true) {
                var fNext = queue.Dequeue();
                yield return fNext;

                var msg = fNext.Result;
                // Result intentionally discarded
                Program.CallFunction(process, "common.messaging", "notifyNewMessage", msg.Source.Process.Id, msg.Data);
            }
        }

        private IEnumerator<object> LogTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("log");
            var sleep = new Sleep(0.01);

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                LogPrint(process, fNext.Result.DecodeAsciiZ());

                yield return sleep;
            }
        }

        protected void LogPrint (ProcessInfo process, string text, params object[] arguments) {
            LogPrint(process, String.Format(text, arguments));
        }

        public void LogPrint (ProcessInfo process, string text) {
            string logText;
            if (process != null)
                logText = String.Format("{1:HH:mm:ss} {0}: {2}", process.Process.Id, DateTime.Now, text);
            else
                logText = String.Format("{0:HH:mm:ss}: {1}", DateTime.Now, text);

            Log.Add(logText);
            Console.WriteLine(logText);
            if (LogWindowInstance != null)
                LogWindowInstance.AddLine(logText);
        }

        public void LogClear () {
            Log.Clear();
            if (LogWindowInstance != null)
                LogWindowInstance.Clear();
            LogPrint(null, "Log cleared");
        }

        private IEnumerator<object> RemoteCallTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("remotecall");
            var serializer = new JavaScriptSerializer();

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                object[] callTuple = serializer.Deserialize<object[]>(fNext.Result.DecodeAsciiZ());
                string scriptName = callTuple[0] as string;
                string methodName = callTuple[1] as string;
                object[] rawArguments = callTuple[2] as object[];

                object[] functionArguments;
                if (rawArguments == null) {
                    functionArguments = new object[] { process };
                } else {
                    functionArguments = new object[rawArguments.Length + 1];

                    functionArguments[0] = process;

                    for (int i = 0; i < rawArguments.Length; i++)
                        functionArguments[i + 1] = rawArguments[i];
                }

                IManagedScript instance = Program.GetScriptInstance(
                    new ScriptName(scriptName)
                );

                if (instance == null) {
                    LogPrint(process,
                        "Remote call attempted on script '{0}' that isn't loaded.", scriptName
                    );
                    continue;
                }

                process.Start(RemoteCallInvoker(
                    instance, methodName, functionArguments, rawArguments, serializer, scriptName, process
                ));
            }
        }

        protected IEnumerator<object> RemoteCallInvoker (IManagedScript instance, string methodName, object[] functionArguments, object[] rawArguments, JavaScriptSerializer serializer, string scriptName, ProcessInfo process) {
            IEnumerator<object> resultTask = null;

            try {
                resultTask = instance.GetType().InvokeMember(
                    methodName, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, instance, functionArguments
                ) as IEnumerator<object>;
            } catch (Exception ex) {
                LogPrint(process,
                    "Remote call '{0}.{1}' with args {2} failed with exception: {3}",
                    scriptName, methodName, serializer.Serialize(rawArguments), ex.ToString()
                );
            }

            if (resultTask != null)
                yield return resultTask;
            else
                yield break;
        }

        public void LoggedInCharacterChanged (ProcessInfo process, object characterName) {
            process.Status = characterName as string ?? "Not Logged In";

            EventBus.Broadcast(Profile, "RunningProcessChanged", process);
        }

        public override IEnumerator<object> Reload () {
            LogPrint(null, "Scripts loaded");

            yield break;
        }

        public void ShowMessageBox (ProcessInfo process, string title, string text) {
            MessageBox.Show(text, title);
        }

        public void ShowBalloonTip (ProcessInfo process, string title, string text) {
            ShowBalloonTip(process, 60000, title, text);
        }

        public void ShowBalloonTip (ProcessInfo process, int timeout, string title, string text) {
            Program.TrayIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.Info);
        }

        public IEnumerator<object> PlaySound (ProcessInfo process, string pythonModuleName, string filename) {
            string scriptPath = Path.GetDirectoryName(Application.ExecutablePath);
            if (pythonModuleName != null) {
                ScriptName sn;
                if (Program.PythonModuleToScript.TryGetValue(pythonModuleName, out sn)) {
                    var fn = Program.FindScript(sn);
                    if (fn != null)
                        scriptPath = fn.Directory;
                }
            }

            var soundPath = Path.Combine(scriptPath, filename);
            if (!File.Exists(soundPath)) {
                LogPrint(process, "PlaySound request failed because the sound '{0}' was not found.", soundPath);
                yield break;
            }

            SoundPlayer sp = new SoundPlayer();

            var f = new SignalFuture();
            sp.SoundLocation = soundPath;
            sp.LoadCompleted += (s, e) => {
                if (e.Error != null)
                    f.Fail(e.Error);
                else
                    f.Complete();
            };
            sp.LoadAsync();

            yield return f;

            if (f.Failed) {
                LogPrint(process, "PlaySound request failed because the sound '{0}' could not be loaded: {1}", soundPath, f.Error);
                yield break;
            }

            Console.WriteLine("Playing {0}...", soundPath);
            using (sp)
                yield return Future.RunInThread(sp.PlaySync);

            Console.WriteLine("Done playing {0}.", soundPath);
        }
    }
}
