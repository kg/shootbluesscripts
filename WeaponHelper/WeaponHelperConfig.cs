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
    public partial class WeaponHelperConfig : WeaponHelperConfigPanel {
        public WeaponHelperConfig (WeaponHelper script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => PreferMyTargets.Checked),
                BoundMember.New(() => MinimumChanceToHit.Value)
            };
        }
    }

    public class WeaponHelperConfigPanel : SimpleConfigPanel<WeaponHelper> {
        public WeaponHelperConfigPanel ()
            : base(null) {
        }

        public WeaponHelperConfigPanel (WeaponHelper script)
            : base(script) {
        }
    }
}
