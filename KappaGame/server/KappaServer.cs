using Kappa.entity;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kappa.server
{
    class KappaServer
    {
        PlayerManager pManager = new PlayerManager();
        
        private NetServer server;
        private NetPeerConfiguration config;
        private Thread messageThread;
        private bool isListening = false;

        public int ConnectionPort { get { return config.Port; } }

        public KappaServer(List<PlayerModel> playerList = null) {
            messageThread = new Thread(ListenForMessages);
            config = new NetPeerConfiguration("KappaGame");
            config.Port = 42069;
            server = new NetServer(config);

            if (playerList != null)
                AddPlayers(playerList);
        }

        public void AddPlayers(List<PlayerModel> playerList) {
            //pManager.Players.Add((PlayerModel) (from p in playerList select p));
            foreach (Player p in playerList) {
                pManager.Players.Add(p);
            }
        }

        public void Start() {
            server.Start();
            isListening = true;
            messageThread.Start();
        }

        public void Stop() {
            isListening = false;
            messageThread.Join();
            server.Shutdown("Server has stopped");
        }

        private void ListenForMessages() {
            NetIncomingMessage message;
            Console.WriteLine("SERVER: Listening for messages");
            while (isListening) {
                while ((message = server.ReadMessage()) != null) {
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            Console.WriteLine("SERVER: Read Data here");
                            Console.WriteLine(message.ReadString());
                            //var data = message.Read * ();
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            //switch (message.SenderConnection.Status) {
                            /* .. */
                            //}
                            Console.WriteLine($"SERVER: Status Changed = {message.SenderConnection.Status}");
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine($"SERVER: {message.ReadString()}");
                            break;

                        /* .. */
                        case NetIncomingMessageType.WarningMessage:
                            Console.WriteLine($"SERVER: WARNING: {message}");
                            break;

                        default:
                            Console.WriteLine("SERVER: unhandled message with type: "
                                + message.MessageType);
                            break;
                    }
                }
            }
            Console.WriteLine("SERVER: Done listening for messages");
        }
    }
}
