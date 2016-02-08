using Kappa.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kappa.server
{
    class Server
    {
        PlayerManager pManager = new PlayerManager();

        public Server(List<PlayerModel> playerList = null) {
        }

        public void AddPlayers(List<PlayerModel> playerList) {
            if (playerList != null) {
                pManager.Players.Add((PlayerModel) (from p in playerList select p));
                /*
                foreach (PlayerModel pModel in playerList) {
                    pManager.Players.Add(pModel);
                }
                */
            }
        }

        public void Run() {
            Console.WriteLine("Running Server");
            Thread t = new Thread(pManager.ListenForPlayers);
            t.Start();
        }
    }
}
