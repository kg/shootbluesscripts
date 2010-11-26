using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using Squared.Util.Event;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace ShootBlues.Script {
    public class BroadcastHelper : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public BroadcastHelper (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("TargetColors.script.dll", true);
            AddDependency("broadcasthelper.py");

            CustomMenu = new ToolStripMenuItem("Broadcast Helper");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureBroadcastHelper);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureBroadcastHelper (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Broadcast Helper"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new BroadcastHelperConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }

            yield return BaseInitialize();
        }

        public override IEnumerator<object> Reload () {
            var targetColors = Program.GetScriptInstance<TargetColors>("TargetColors.script.dll");

            if (targetColors != null) {
                yield return targetColors.DefineColor("Broadcast: Target", Color.FromArgb(255, 128, 180));
                yield return targetColors.DefineColor("Broadcast: Need Shield", Color.FromArgb(128, 180, 255));
                yield return targetColors.DefineColor("Broadcast: Need Armor", Color.FromArgb(255, 180, 128));
                yield return targetColors.DefineColor("Broadcast: Need Capacitor", Color.FromArgb(180, 255, 128));
            }
        }

        protected override IEnumerator<object> OnPreferenceChanged (EventInfo evt, string prefName) {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "broadcasthelper", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "broadcasthelper", "initialize");

            EventBus.Broadcast(this, "PreferenceChanged", "*");
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new BroadcastHelperConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Broadcast Helper", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Broadcast Helper");
            yield break;
        }
    }
}
