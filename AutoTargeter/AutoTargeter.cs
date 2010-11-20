using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ShootBlues.Script {
    public class AutoTargeter : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public AutoTargeter (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("EnemyPrioritizer.script.dll");
            AddDependency("autotargeter.py");

            CustomMenu = new ToolStripMenuItem("Auto Targeter");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureAutoTargeter);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureAutoTargeter (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Auto Targeter"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            /*
            // Hack to initialize prefs to defaults
            using (var configWindow = new AutoTargeterConfig(this)) {
                yield return configWindow.LoadPreferences();
                yield return configWindow.SavePreferences();
            }
             */

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferencesChanged () {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "autotargeter", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "autotargeter", "initialize");

            yield return OnPreferencesChanged();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new Control(); // new AutoTargeterConfig(this);
            // yield return panel.LoadPreferences();
            statusWindow.ShowConfigurationPanel("Auto Targeter", panel);
            yield break;
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Auto Targeter");
            yield break;
        }
    }
}
