using Kappa.server.packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.server {
    abstract class PlayerConnection {

        public enum ConnectionType {
            ONLINE,
            OFFLINE
        }

        public ConnectionType Type { get; protected set; }
    }
}
