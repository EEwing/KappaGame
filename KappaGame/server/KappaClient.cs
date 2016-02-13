using Kappa.entity;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kappa.server
{
    class KappaClient
    {
        PlayerManager pManager = new PlayerManager();

        public int ConnectionPort { get; set; } = 25565; // Maybe should go in online server...
        private NetClient client;
        private NetPeerConfiguration config;
        private Thread messageThread;
        private bool isListening = false;

        public KappaClient(List<PlayerModel> playerList = null) {
            messageThread = new Thread(ListenForMessages);
            config = new NetPeerConfiguration("KappaGame");
            client = new NetClient(config);

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
            client.Start();
            isListening = true;
            messageThread.Start();
        }

        public void Stop() {
            isListening = false;
            messageThread.Join();
            client.Shutdown("Server has stopped");
        }

        private void ListenForMessages() {
            NetIncomingMessage message;
            Console.WriteLine("CLIENT: Listening for messages");
            while (isListening) {
                while ((message = client.ReadMessage()) != null) {
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            Console.WriteLine("CLIENT: Read Data here");
                            Console.WriteLine(message.ReadString());
                            //var data = message.Read * ();
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            //switch (message.SenderConnection.Status) {
                            /* .. */
                            //}
                            Console.WriteLine($"CLIENT: Status Changed = {message.SenderConnection.Status}");
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine($"CLIENT: {message.ReadString()}");
                            break;

                        /* .. */
                        case NetIncomingMessageType.WarningMessage:
                            Console.WriteLine($"CLIENT: WARNING: {message}");
                            break;

                        default:
                            Console.WriteLine("CLIENT: unhandled message with type: "
                                + message.MessageType);
                            break;
                    }
                }
            }
            Console.WriteLine("CLIENT: Done listening for messages");
        }

        // Blocking function! returns once connection is established
        public void ConnectToServer(string host, int port) {
            client.Connect(host: host, port: port);
            while (client.ServerConnection == null) { }
        }

        // Dummy function to test connection to server
        public void SendString (string toSend) {
            var Message = client.CreateMessage();
            Message.Write(toSend);
            client.SendMessage(Message, NetDeliveryMethod.ReliableOrdered);
        } 
    }
}
