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
        }

        private IEnumerator<object> LogTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("log");

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                var logText = String.Format("{0}: {1}", process.Process.Id, fNext.Result.DecodeAsciiZ());
                Log.Add(logText);
                Console.WriteLine(logText);
                if (LogWindowInstance != null)
                    LogWindowInstance.AddLine(logText);
            }
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
                object[] functionArguments = callTuple[2] as object[];
                Type[] argumentTypes;

                if (functionArguments == null)
                    argumentTypes = new Type[0];
                else {
                    argumentTypes = new Type[functionArguments.Length];
                    for (int i = 0; i < argumentTypes.Length; i++)
                        argumentTypes[i] = functionArguments[i].GetType();
                }

                IManagedScript instance = Program.GetScriptInstance(
                    new ScriptName(scriptName)
                );

                if (instance == null) {
                    Console.WriteLine("Remote call attempted on script '{0}' that isn't loaded.", scriptName);
                    continue;
                }

                var method = instance.GetType().GetMethod(
                    methodName, argumentTypes
                );
                if (method == null) {
                    Console.WriteLine("Remote call attempted on script '{0}', but no method was found with the name '{1}' that could accept the arguments {2}.", scriptName, methodName, serializer.Serialize(functionArguments));
                    continue;
                }

                try {
                    object result = method.Invoke(instance, functionArguments);
                    var resultTask = result as IEnumerator<object>;
                    if (resultTask != null) {
                        process.Start(resultTask);
                        Console.WriteLine("Remote call '{0}.{1}' with args {2} started a task.", scriptName, methodName, serializer.Serialize(functionArguments));
                    } else {
                        Console.WriteLine("Remote call '{0}.{1}' with args {2} completed with result: {3}", scriptName, methodName, serializer.Serialize(functionArguments), serializer.Serialize(result));
                    }
                } catch (Exception ex) {
                    Console.WriteLine("Remote call '{0}.{1}' with args {2} failed with exception: {3}", scriptName, methodName, serializer.Serialize(functionArguments), ex.ToString());
                }
            }
        }

        public void ShowMessageBox (string text) {
            MessageBox.Show(text);
        }

        public void ShowMessageBox (string text, string caption) {
            MessageBox.Show(text, caption);
        }
    }
}
