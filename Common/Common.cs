using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ShootBlues.Script {
    public class Common : IManagedScript {
        string ScriptPath;

        public Common () {
            ScriptPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "common.py"
            );
        }

        public void Dispose () {
        }

        public static IEnumerator<object> CreateNamedChannel (ProcessInfo process, string name) {
            var channel = process.GetNamedChannel(name);

            yield return Program.CallFunction(process, "common", "_initChannel", String.Format("[\"{0}\", {1}]", name, channel.ChannelID));
        }

        public IEnumerator<object> LoadInto (ProcessInfo process) {
            Console.WriteLine("Common.LoadInto {0}", process.Process.Id);

            yield return Program.LoadScriptFromFilename(process, ScriptPath);
        }

        public IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return CreateNamedChannel(process, "log");

            process.Start(LogTask(process));
        }

        private IEnumerator<object> LogTask (ProcessInfo process) {
            var channel = process.GetNamedChannel("log");

            while (true) {
                var fNext = channel.Receive();
                yield return fNext;

                Console.WriteLine("{0}: {1}", process.Process.Id, Encoding.ASCII.GetString(fNext.Result));
            }
        }

        public IEnumerator<object> UnloadFrom (ProcessInfo process) {
            Console.WriteLine("Common.UnloadFrom {0}", process.Process.Id);

            yield return Program.UnloadScriptFromFilename(process, ScriptPath);
        }

        public IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            yield break;
        }

        public IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            yield break;
        }
    }
}
