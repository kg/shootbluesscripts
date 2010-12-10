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

namespace ShootBlues.Script {
    public class Common : ManagedScript {
        public struct MessageData {
            public readonly ProcessInfo Source;
            public readonly Dictionary<string, object> Data;

            public MessageData (ProcessInfo source, Dictionary<string, object> data) {
                Source = source;
                Data = data;
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
                Type[] argumentTypes;

                object[] functionArguments;
                if (rawArguments == null) {
                    argumentTypes = new Type[] { typeof(ProcessInfo) };
                    functionArguments = new object[] { process };
                } else {
                    argumentTypes = new Type[rawArguments.Length + 1];
                    functionArguments = new object[argumentTypes.Length];

                    functionArguments[0] = process;
                    argumentTypes[0] = typeof(ProcessInfo);

                    for (int i = 0; i < rawArguments.Length; i++) {
                        functionArguments[i + 1] = rawArguments[i];
                        if (rawArguments[i] != null)
                            argumentTypes[i + 1] = rawArguments[i].GetType();
                        else
                            argumentTypes[i + 1] = typeof(object);
                    }
                }

                IManagedScript instance = Program.GetScriptInstance(
                    new ScriptName(scriptName)
                );

                if (instance == null) {
                    LogPrint(process, String.Format(
                        "Remote call attempted on script '{0}' that isn't loaded.", scriptName
                    ));
                    continue;
                }

                var method = instance.GetType().GetMethod(
                    methodName, argumentTypes
                );
                if (method == null) {
                    LogPrint(process, String.Format(
                        "Remote call attempted on script '{0}', but no method was found with the name '{1}' that could accept the arguments {2}.", 
                        scriptName, methodName, serializer.Serialize(rawArguments)
                    ));
                    continue;
                }

                try {
                    object result = method.Invoke(instance, functionArguments);
                    var resultTask = result as IEnumerator<object>;
                    if (resultTask != null)
                        process.Start(resultTask);
                } catch (Exception ex) {
                    LogPrint(process, String.Format(
                        "Remote call '{0}.{1}' with args {2} failed with exception: {3}", 
                        scriptName, methodName, serializer.Serialize(rawArguments), ex.ToString()
                    ));
                }
            }
        }

        public void LoggedInCharacterChanged (ProcessInfo process, object characterName) {
            process.Status = characterName as string ?? "Not Logged In";

            EventBus.Broadcast(Profile, "RunningProcessChanged", process);
        }

        public override IEnumerator<object> Reload () {
            LogPrint(null, "Scripts loaded");

            yield break;
        }
    }
}
