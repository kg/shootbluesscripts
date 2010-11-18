using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShootBlues;
using Squared.Util.Bind;
using System.IO;
using System.Web.Script.Serialization;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class DroneHelperConfig : UserControl {
        IBoundMember[] Prefs;
        DroneHelper Script;

        public DroneHelperConfig (DroneHelper script) {
            InitializeComponent();
            Script = script;

            Prefs = new IBoundMember[] {
                BoundMember.New(() => AutoAttackWhenIdle.Checked),
                BoundMember.New(() => AutoAttackWhenTargetLost.Checked),
                BoundMember.New(() => Largest.Checked),
                BoundMember.New(() => Smallest.Checked),
                BoundMember.New(() => ClosestToDrones.Checked),
                BoundMember.New(() => RecallIfShieldsBelow.Checked),
                BoundMember.New(() => RecallShieldThreshold.Value),
            };

            Program.Scheduler.Start(LoadPreferences(), TaskExecutionPolicy.RunAsBackgroundTask);
        }

        private void RecallIfShieldsBelow_CheckedChanged (object sender, EventArgs e) {
            RecallShieldThreshold.Enabled = RecallIfShieldsBelow.Checked;
            ValuesChanged(sender, e);
        }

        public string GetPrefsPath () {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Path.Combine("ShootBlues", "DroneHelperPreferences.json")
            );
        }

        public string GetMemberName (IBoundMember member) {
            return ((Control)member.Target).Name;
        }

        public IEnumerator<object> LoadPreferences () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(Script.GetPreferences());
            yield return rtc;

            var dict = rtc.Result;
            object value;

            foreach (var bm in Prefs)
                if (dict.TryGetValue(GetMemberName(bm), out value))
                    bm.Value = value;
        }

        public IEnumerator<object> SavePreferences () {
            using (var xact = Program.Database.CreateTransaction()) {
                yield return xact;

                foreach (var bm in Prefs)
                    yield return Script.SetPreference(GetMemberName(bm), bm.Value);

                yield return xact.Commit();
            }
        }

        private void ValuesChanged (object sender, EventArgs args) {
            Program.Scheduler.Start(
                SavePreferences(), TaskExecutionPolicy.RunAsBackgroundTask
            );
        }
    }
}
