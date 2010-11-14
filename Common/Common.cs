using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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

            yield return Program.CallFunction(process, "common", "_initChannel", String.Format("[\"{0}\", {1}]", name, channel.ChannelID));
        }

        override public IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return CreateNamedChannel(process, "log");

            process.Start(LogTask(process));
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
    }
}
