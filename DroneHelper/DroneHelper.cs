using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ShootBlues.Script {
    public class DroneHelper : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public DroneHelper (ScriptName name)
            : base (name) {

            AddDependency("Common.script.dll");
            AddDependency("dronehelper.py");

            CustomMenu = new ToolStripMenuItem("Drone Helper");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureDroneHelper);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureDroneHelper (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Drone Helper"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Common.CreateNamedChannel(process, "dronehelper");

            yield return Program.CallFunction(process, "dronehelper", "initialize");
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new DroneHelperConfig();
            statusWindow.ShowConfigurationPanel("Drone Helper", panel);
            yield break;
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Drone Helper");
            yield break;
        }
    }
}
