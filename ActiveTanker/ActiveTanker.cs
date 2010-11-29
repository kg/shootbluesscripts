using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.IO;
using Squared.Util.Event;

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

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new ActiveTankerConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            string prefsJson = null;
            yield return Preferences.GetAllJson().Bind(() => prefsJson);

            yield return CallFunction("activetanker", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "activetanker", "initialize");

            Preferences.Flush();
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
