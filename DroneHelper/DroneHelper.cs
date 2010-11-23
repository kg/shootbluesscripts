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
            AddDependency("EnemyPrioritizer.script.dll");
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

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new DroneHelperConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferencesChanged () {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "dronehelper", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "dronehelper", "initialize");

            yield return OnPreferencesChanged();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new DroneHelperConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Drone Helper", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Drone Helper");
            yield break;
        }
    }
}
