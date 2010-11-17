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
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("enemyprioritizer.py");

            CustomMenu = new ToolStripMenuItem("Enemy Prioritizer");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureEnemyPrioritizer);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureEnemyPrioritizer (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Enemy Prioritizer"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            /*
            // Hack to initialize prefs to defaults
            using (var configWindow = new DroneHelperConfig(this))
                yield return configWindow.SavePreferences();
             */

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferencesChanged () {
            var prefsJson = GetPreferencesJson();

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "enemyprioritizer", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "enemyprioritizer", "initialize");

            var prefsJson = GetPreferencesJson();
            yield return Program.CallFunction(process, "enemyprioritizer", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new Control();
            statusWindow.ShowConfigurationPanel("Enemy Prioritizer", panel);
            yield break;
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Enemy Prioritizer");
            yield break;
        }
    }
}
