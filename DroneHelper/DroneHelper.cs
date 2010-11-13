using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ShootBlues.Script {
    public class DroneHelper : IManagedScript {
        string CommonPath;
        string ScriptPath;
        ToolStripMenuItem CustomMenu;

        public DroneHelper () {
            CommonPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Common.dll"
            );

            ScriptPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "dronehelper.py"
            );

            CustomMenu = new ToolStripMenuItem("Drone Helper");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureDroneHelper);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public void Dispose () {
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureDroneHelper (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Drone Helper"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public IEnumerator<object> LoadInto (ProcessInfo process) {
            Console.WriteLine("DroneHelper.LoadInto {0}", process.Process.Id);

            yield return Program.LoadScriptFromFilename(
                process, CommonPath
            );

            yield return Program.LoadScriptFromFilename(
                process, ScriptPath
            );
        }

        public IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Common.CreateNamedChannel(process, "dronehelper");
        }

        public IEnumerator<object> UnloadFrom (ProcessInfo process) {
            Console.WriteLine("DroneHelper.UnloadFrom {0}", process.Process.Id);
            
            yield return Program.UnloadScriptFromFilename(
                process, CommonPath
            );

            yield return Program.UnloadScriptFromFilename(
                process, ScriptPath
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
