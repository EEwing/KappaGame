using Kappa.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.server {
    class PlayerManager {
        public List<PlayerModel> Players { get; internal set; }

        public PlayerManager() {
            Players = new List<PlayerModel>();
        }

        public void ListenForPlayers() {

        }

    }
}
