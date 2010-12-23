using System;
using ShootBlues;
using System.Collections.Generic;
using System.Diagnostics;
using Squared.Task;
using System.Reflection;
using System.IO;

namespace ShootBlues.Profile {
    public class EVE : SimpleExecutableProfile {
        public Dictionary<int, IntPtr> ProcessWindows = new Dictionary<int, IntPtr>();

        public EVE ()
            : base("exefile.exe") {

            AddDependency("Common.Script.dll");
        }

        public override string ProfileName {
            get {
                return "EVE Online";
            }
        }

        private IEnumerator<object> BaseOnNewProcess (Process process) {
            return base.OnNewProcess(process);
        }

        protected override IEnumerator<object> OnNewProcess (Process process) {
            Console.WriteLine("Waiting for EVE to start.");

            IntPtr hWnd = IntPtr.Zero;
            while ((hWnd == IntPtr.Zero) && (!process.HasExited)) {
                yield return new Sleep(1);

                hWnd = Win32.FindProcessWindow(process.Id, "triuiScreen", "EVE");
            }

            if (hWnd == IntPtr.Zero) {
                Console.WriteLine("EVE exited without starting (crashed?)");
                yield break;
            }

            yield return new Sleep(10);

            ProcessWindows[process.Id] = hWnd;
            yield return BaseOnNewProcess(process);
        }

        public override IEnumerator<object> WaitUntilProcessReady (ProcessInfo process) {
            bool isReady = false;
            do {
                yield return Program.EvalPython<bool>(process, @"
try:
  m = __import__('uix')
  g = getattr(__import__('__builtin__'), 'sm', None)
  return (m is not None) and (g is not None)
except:
  return False").Bind(() => isReady);

                yield return new Sleep(0.5);
            } while (!isReady);

            Console.WriteLine("EVE started.");
        }
    }
}
