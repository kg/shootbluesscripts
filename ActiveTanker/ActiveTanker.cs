using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ShootBlues.Script {
    public class ActiveTanker : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public ActiveTanker (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("activetanker.py");

            CustomMenu = new ToolStripMenuItem("Active Tanker");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureActiveTanker);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureActiveTanker (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Active Tanker"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new ActiveTankerConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferencesChanged () {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "activetanker", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "activetanker", "initialize");

            yield return OnPreferencesChanged();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new ActiveTankerConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Active Tanker", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Active Tanker");
            yield break;
        }
    }
}
