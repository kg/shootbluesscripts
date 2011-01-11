using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootBlues.Script {
    public interface IMessageGateway {
        string[] GetEndpoints ();
        bool Send (string endpoint, string message);
    }
}
