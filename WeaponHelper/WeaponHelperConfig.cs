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
    public partial class WeaponHelperConfig : SimpleConfigPanel<WeaponHelper> {
        public WeaponHelperConfig (WeaponHelper script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
            };
        }
    }
}
