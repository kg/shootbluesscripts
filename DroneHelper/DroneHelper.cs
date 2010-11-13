using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;

namespace DroneHelper {
    public class DroneHelper : IManagedScript {
        ToolStripMenuItem CustomMenu;

        public DroneHelper () {
            CustomMenu = new ToolStripMenuItem("Drone Helper");
            Program.AddCustomMenu(CustomMenu);
        }

        public void Dispose () {
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public IEnumerator<object> LoadInto (ProcessInfo process) {
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

        public IEnumerator<object> UnloadFrom (ProcessInfo process) {
            yield return Program.UnloadScriptByModuleName(
                process, "dronehelper"
            );
        }

        public IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new DroneHelperConfig();
            statusWindow.ShowConfigurationPanel("Drone Helper", panel);
            yield break;
        }

        public IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Drone Helper");
            yield break;
        }
    }
}
