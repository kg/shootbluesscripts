using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Squared.Task;
using Squared.Task.Data.Mapper;
using Squared.Util.Bind;

namespace ShootBlues.Script {
    public partial class GoonfleetGatewayConfig : GoonfleetGatewayConfigPanel {
        public GoonfleetGatewayConfig (GoonfleetGateway script)
            : base(script) {
            InitializeComponent();

            Prefs = new IBoundMember[] {
                BoundMember.New(() => URI.Text),
                BoundMember.New(() => Username.Text),
                BoundMember.New(() => Key.Text),
                BoundMember.New(() => Target.Text)
            };
        }
    }

    public class GoonfleetGatewayConfigPanel : SimpleConfigPanel<GoonfleetGateway> {
        public GoonfleetGatewayConfigPanel ()
            : base(null) {
        }

        public GoonfleetGatewayConfigPanel (GoonfleetGateway script)
            : base(script) {
        }
    }
}
