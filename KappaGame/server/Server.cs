using Kappa.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kappa.server
{
    abstract class Server
    {
        PlayerManager pManager = new PlayerManager();

        public int ConnectionPort { get; set; } = 80; // Maybe should go in online server...

        public Server(List<PlayerModel> playerList = null) {
        }

        public void AddPlayers(List<PlayerModel> playerList) {
            //pManager.Players.Add((PlayerModel) (from p in playerList select p));
            foreach (Player p in playerList) {
                pManager.Players.Add(p);
            }
        }

        public virtual void Run() {
            Console.WriteLine("Running Server");
            Thread t = new Thread(pManager.ListenForPlayers);
            t.Start();
        }
    }
}
