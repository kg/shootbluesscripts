using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using Squared.Util.Bind;

namespace ShootBlues.Script {
    public partial class AutoTargeterConfig : TaskUserControl, IConfigurationPanel {
        IBoundMember[] Prefs;
        AutoTargeter Script;

        public AutoTargeterConfig (AutoTargeter script)
            : base (Program.Scheduler) {
            InitializeComponent();
            Script = script;

            Prefs = new IBoundMember[] {
                BoundMember.New(() => TargetNeutralPlayers.Checked),
                BoundMember.New(() => TargetHostilePlayers.Checked),
                BoundMember.New(() => TargetHostileNPCs.Checked),
                BoundMember.New(() => ReservedTargetSlots.Value)
            };
        }

        public string GetMemberName (IBoundMember member) {
            return ((Control)member.Target).Name;
        }

        public IEnumerator<object> LoadConfiguration () {
            var rtc = new RunToCompletion<Dictionary<string, object>>(Script.GetPreferences());
            yield return rtc;

            var dict = rtc.Result;
            object value;

            foreach (var bm in Prefs)
                if (dict.TryGetValue(GetMemberName(bm), out value))
                    bm.Value = value;
        }

        public IEnumerator<object> SaveConfiguration () {
            using (var xact = Program.Database.CreateTransaction()) {
                yield return xact;

                foreach (var bm in Prefs)
                    yield return Script.SetPreference(GetMemberName(bm), bm.Value);

                yield return xact.Commit();
            }
        }

        private void ValuesChanged (object sender, EventArgs args) {
            Start(SaveConfiguration());
        }
    }
}
