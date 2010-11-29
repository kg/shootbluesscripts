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

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new WeaponHelperConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            string prefsJson = null;
            yield return Preferences.GetAllJson().Bind(() => prefsJson);

            yield return CallFunction("weaponhelper", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "weaponhelper", "initialize");

            Preferences.Flush();
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
