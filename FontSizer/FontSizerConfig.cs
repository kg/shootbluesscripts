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
    public partial class FontSizerConfig : FontSizerConfigPanel {
        public FontSizerConfig (FontSizer script)
            : base (script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => FontScale.Value),
            };
        }

        private void FontScale_ValueChanged (object sender, EventArgs e) {
            FontSizeValue.Text = String.Format("{0}%", FontScale.Value);
        }
    }

    public class FontSizerConfigPanel : SimpleConfigPanel<FontSizer> {
        public FontSizerConfigPanel ()
            : base(null) {
        }

        public FontSizerConfigPanel (FontSizer script)
            : base(script) {
        }
    }
}
