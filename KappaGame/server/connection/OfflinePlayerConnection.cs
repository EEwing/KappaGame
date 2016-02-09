using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kappa.server.packet;
using Kappa.entity;
using Kappa.world;

namespace Kappa.server.connection {
    class OfflinePlayerConnection : PlayerConnection {

        PlayerModel player;
        MapModel map;

        public OfflinePlayerConnection() {
            Type = ConnectionType.OFFLINE;
        }

        public override void SendMap(MapModel serverMap) {
            map = serverMap;
        }

        public override void SendPlayerModel(PlayerModel serverPlayer) {
            player = serverPlayer; // Overwrite pointer?
        }
    }
}
