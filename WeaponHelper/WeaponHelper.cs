using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.IO;
using Squared.Util.Event;

namespace ShootBlues.Script {
    public class WeaponHelper : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public WeaponHelper (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("weaponhelper.py");

            CustomMenu = new ToolStripMenuItem("Weapon Helper");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureWeaponHelper);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureWeaponHelper (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Weapon Helper"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        protected IEnumerator<object> BaseInitialize () {
            return base.Initialize();
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new WeaponHelperConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }

            yield return BaseInitialize();
        }

        protected override IEnumerator<object> OnPreferenceChanged (EventInfo evt, string prefName) {
            string prefsJson = null;
            yield return GetPreferencesJson().Bind(() => prefsJson);

            foreach (var process in Program.RunningProcesses)
                yield return Program.CallFunction(process, "weaponhelper", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "weaponhelper", "initialize");

            EventBus.Broadcast(this, "PreferenceChanged", "*");
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new WeaponHelperConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Weapon Helper", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Weapon Helper");
            yield break;
        }
    }
}
