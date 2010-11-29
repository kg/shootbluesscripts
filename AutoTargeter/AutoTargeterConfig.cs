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
    public partial class AutoTargeterConfig : AutoTargeterConfigPanel {
        public AutoTargeterConfig (AutoTargeter script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => TargetNeutralPlayers.Checked),
                BoundMember.New(() => TargetHostilePlayers.Checked),
                BoundMember.New(() => TargetFriendlyPlayers.Checked),
                BoundMember.New(() => TargetHostileNPCs.Checked),
                BoundMember.New(() => ReservedTargetSlots.Value)
            };
        }
    }

    public class AutoTargeterConfigPanel : SimpleConfigPanel<AutoTargeter> {
        public AutoTargeterConfigPanel ()
            : base(null) {
        }

        public AutoTargeterConfigPanel (AutoTargeter script)
            : base(script) {
        }
    }
}
