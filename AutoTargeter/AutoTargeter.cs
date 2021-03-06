﻿using System;
using System.Collections.Generic;
using System.Text;
using ShootBlues;
using Squared.Task;
using System.Windows.Forms;
using Squared.Util.Event;
using System.IO;
using System.Drawing;

namespace ShootBlues.Script {
    public class AutoTargeter : ManagedScript {
        ToolStripMenuItem CustomMenu;

        public AutoTargeter (ScriptName name)
            : base(name) {

            AddDependency("Common.script.dll");
            AddDependency("EnemyPrioritizer.script.dll");
            AddDependency("TargetColors.script.dll", true);
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

        public override IEnumerator<object> Initialize () {
            // Hack to initialize prefs to defaults
            using (var configWindow = new AutoTargeterConfig(this)) {
                yield return configWindow.LoadConfiguration();
                yield return configWindow.SaveConfiguration();
            }
        }

        public override IEnumerator<object> Reload () {
            var targetColors = Program.GetScriptInstance<TargetColors>("TargetColors.script.dll");

            if (targetColors != null)
                targetColors.DefineColor("Automatic Target", Color.FromArgb(255, 220, 180));

            yield break;
        }

        protected override IEnumerator<object> OnPreferencesChanged (EventInfo evt, string[] prefNames) {
            string prefsJson = null;
            yield return Preferences.GetAllJson().Bind(() => prefsJson);

            yield return CallFunction("autotargeter", "notifyPrefsChanged", prefsJson);
        }

        public override IEnumerator<object> LoadedInto (ProcessInfo process) {
            yield return Program.CallFunction(process, "autotargeter", "initialize");

            Preferences.Flush();
        }

        public override IEnumerator<object> OnStatusWindowShown (IStatusWindow statusWindow) {
            var panel = new AutoTargeterConfig(this);
            yield return panel.LoadConfiguration();
            statusWindow.ShowConfigurationPanel("Auto Targeter", panel);
        }

        public override IEnumerator<object> OnStatusWindowHidden (IStatusWindow statusWindow) {
            statusWindow.HideConfigurationPanel("Auto Targeter");
            yield break;
        }
    }
}
