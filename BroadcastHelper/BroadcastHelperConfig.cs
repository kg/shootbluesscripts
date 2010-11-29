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
    public partial class BroadcastHelperConfig : BroadcastHelperConfigPanel {
        public BroadcastHelperConfig (BroadcastHelper script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => TargetPriorityBoost.Value),
                BoundMember.New(() => RepPriorityBoost.Value)
            };
        }
    }

    public class BroadcastHelperConfigPanel : SimpleConfigPanel<BroadcastHelper> {
        public BroadcastHelperConfigPanel ()
            : base(null) {
        }

        public BroadcastHelperConfigPanel (BroadcastHelper script)
            : base(script) {
        }
    }
}
