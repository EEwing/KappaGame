using Kappa.entity;
using Kappa.world;
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

        public abstract void SendPlayerModel(PlayerModel player);

        public abstract void SendMap(MapModel map);

        public ConnectionType Type { get; protected set; }
    }
}
