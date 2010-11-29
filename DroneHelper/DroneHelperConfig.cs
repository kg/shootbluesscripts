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
    public partial class DroneHelperConfig : SimpleConfigPanel<DroneHelper> {
        public DroneHelperConfig (DroneHelper script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => WhenIdle.Checked),
                BoundMember.New(() => WhenTargetLost.Checked),
                BoundMember.New(() => RecallIfShieldsBelow.Checked),
                BoundMember.New(() => RecallShieldThreshold.Value),
                BoundMember.New(() => RedeployWhenShieldsAbove.Checked),
                BoundMember.New(() => RedeployShieldThreshold.Value),
            };
        }

        private void RecallIfShieldsBelow_CheckedChanged (object sender, EventArgs e) {
            RecallShieldThreshold.Enabled = RecallIfShieldsBelow.Checked;
            ValuesChanged(sender, e);
        }

        private void ConfigurePriorities_Click (object sender, EventArgs e) {
            Start(Program.ShowStatusWindow("Enemy Prioritizer"));
        }

        private void RedeployWhenShieldsAbove_CheckedChanged (object sender, EventArgs e) {
            RedeployShieldThreshold.Enabled = RedeployWhenShieldsAbove.Checked;
            ValuesChanged(sender, e);
        }
    }
}
