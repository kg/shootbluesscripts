using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using System.IO;
using Squared.Util.Event;

namespace ShootBlues.Script {
    public class FontSizer : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public FontSizer (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("fontsizer.py");

            CustomMenu = new ToolStripMenuItem("Font Sizer");
            CustomMenu.DropDownItems.Add("Configure", null, ConfigureFontSizer);
            CustomMenu.DropDownItems.Add("-");
            Program.AddCustomMenu(CustomMenu);
        }

        public override void Dispose () {
            base.Dispose();
            Program.RemoveCustomMenu(CustomMenu);
            CustomMenu.Dispose();
        }

        public void ConfigureFontSizer (object sender, EventArgs args) {
            Program.Scheduler.Start(
                Program.ShowStatusWindow("Font Sizer"),
                TaskExecutionPolicy.RunAsBackgroundTask
            );
        }

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new FontSizerConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            long fontScale = 100, fontWidth = 100;
            yield return Preferences.Get<long>("FontScale").Bind(() => fontScale);
            yield return Preferences.Get<long>("FontWidth").Bind(() => fontWidth);

            yield return CallFunction("fontsizer", "setFontSize", fontScale / 100.0f, fontWidth / 100.0f);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "fontsizer", "initialize");

            Preferences.Flush();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new FontSizerConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Font Sizer", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Font Sizer");
            yield break;
        }
    }
}
