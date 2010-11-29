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
    public partial class ActiveTankerConfig : ActiveTankerConfigPanel {
        public ActiveTankerConfig (ActiveTanker script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => KeepShieldsFull.Checked),
                BoundMember.New(() => KeepArmorFull.Checked),
                BoundMember.New(() => KeepStructureFull.Checked),
            };
        }
    }

    public class ActiveTankerConfigPanel : SimpleConfigPanel<ActiveTanker> {
        public ActiveTankerConfigPanel ()
            : base(null) {
        }

        public ActiveTankerConfigPanel (ActiveTanker script)
            : base (script) {
        }
    }
}
