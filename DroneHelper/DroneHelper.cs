using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;

namespace DroneHelper {
    public class DroneHelper : IManagedScript {
        public DroneHelper () {
            Console.WriteLine("DroneHelper()");
        }

        public void Dispose () {
            Console.WriteLine("~DroneHelper()");
        }

        IEnumerator<object> IManagedScript.LoadInto (ProcessInfo process) {
            Console.WriteLine("DroneHelper.LoadInto {0}", process.Process.Id);

            yield return Program.SendScriptText(
                process, "dronehelper",
                @"
def getLockedTargets():
    ballpark = eve.LocalSvc('michelle').GetBallpark()
    targetSvc = sm.services['target']
    return [ballpark.GetInvItem(id) for id in targetSvc.targetsByID.keys()]
"
            );
        }

        IEnumerator<object> IManagedScript.UnloadFrom (ProcessInfo process) {
            Console.WriteLine("DroneHelper.UnloadFrom {0}", process.Process.Id);

            yield return Program.UnloadScriptByModuleName(
                process, "dronehelper"
            );
        }
    }
}
