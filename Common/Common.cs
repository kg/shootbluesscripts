using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace ShootBlues.Script {
    public class Common : ManagedScript {
        protected LogWindow LogWindowInstance = null;

        public List<string> Log = new List<string>();

        ToolStripMenuItem CustomMenu;

        public Common (ScriptName name)
            : base(name) {
            AddDependency("common.py");

            CustomMenu = new ToolStripMenuItem("View Log", null, (s, e) => {
                Program.Scheduler.Start(
                    ShowLogWindow(), Squared.Task.TaskExecutionPolicy.RunAsBackgroundTask
                );
            });
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
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

            process.Start(LogTask(process));
            process.Start(RemoteCallTask(process));

            yield return Program.CallFunction(process, "common", "initialize");
        }

        private IEnumerator<object> LogTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("log");

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                LogPrint(process, fNext.Result.DecodeAsciiZ());
            }
        }

        public void LogPrint (ProcessInfo process, string text) {
            var logText = String.Format("{0} {1:HH:mm:ss}: {2}", process.Process.Id, DateTime.Now, text);
            Log.Add(logText);
            Console.WriteLine(logText);
            if (LogWindowInstance != null)
                LogWindowInstance.AddLine(logText);
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
            Program.RunningProcessesChanged.Set();
        }

        public void ShowMessageBox (ProcessInfo process, string text) {
            MessageBox.Show(text, String.Format("Message from process {0}", process.Process.Id));
        }
    }
}
