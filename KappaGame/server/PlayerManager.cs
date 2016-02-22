using Kappa.entity;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.server {
    class PlayerManager {
        public Dictionary<Guid, NetConnection> PlayerConnections { get; } = new Dictionary<Guid, NetConnection>();
        public List<PlayerModel> Players { get; internal set; }

        public PlayerManager() { 
            Players = new List<PlayerModel>();
        }

        public void ListenForPlayers() {

        }
    }
}
