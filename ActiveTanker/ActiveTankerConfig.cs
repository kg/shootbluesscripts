using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShootBlues;
using Squared.Util.Bind;
using System.IO;
using Squared.Task;

namespace ShootBlues.Script {
    public partial class ActiveTankerConfig : TaskUserControl {
        IBoundMember[] Prefs;
        ActiveTanker Script;

        public ActiveTankerConfig (ActiveTanker script)
            : base (Program.Scheduler) {
            InitializeComponent();
            Script = script;

            Prefs = new IBoundMember[] {
                BoundMember.New(() => KeepShieldsFull.Checked),
                BoundMember.New(() => KeepArmorFull.Checked),
                BoundMember.New(() => KeepStructureFull.Checked),
            };
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
            Start(SavePreferences());
        }
    }
}
