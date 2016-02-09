using Kappa.entity;
using Kappa.server.packet;
using Kappa.world;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.server {
    abstract class PlayerConnection {

        //ClientConnectionContainer connection;
        ClientConnectionContainer connection;

        public enum ConnectionType {
            ONLINE,
            OFFLINE
        }

        public abstract void SendPlayerModel(PlayerModel player);

        public abstract void SendMap(MapModel map);

        public ConnectionType Type { get; protected set; }
    }
}
