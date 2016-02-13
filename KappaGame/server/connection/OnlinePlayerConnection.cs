using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kappa.entity;
using Kappa.world;

namespace Kappa.server.connection {
    class OnlinePlayerConnection : PlayerConnection {

        public OnlinePlayerConnection() {
            Type = ConnectionType.ONLINE;
        }

        public override void SendMap(MapModel map) {
            throw new NotImplementedException();
        }

        public override void SendPlayerModel(PlayerModel serverPlayer) {
            // send data through ports, do funky stuff
            throw new NotImplementedException();
        }
    }
}
