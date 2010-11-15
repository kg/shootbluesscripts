using System;
using ShootBlues;

namespace ShootBlues.Profile {
    public class EVE : SimpleExecutableProfile {
        public EVE ()
            : base("exefile.exe") {
        }

        public override string Name {
            get {
                return "EVE Online";
            }
        }
    }
}
