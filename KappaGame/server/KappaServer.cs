using Kappa.entity;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Shared.Networking;
using Shared.Networking.Interfaces;
using Shared.Networking.Packets.Client;
using Shared.Networking.Packets.Server;

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
            PacketHelper.Add(PacketTypes.Ping, typeof(PingPacket));
            PacketHelper.Add(PacketTypes.Pong, typeof(PongPacket));

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
            Console.WriteLine("SERVER: Listening for messages");
            while (isListening)
            {
                NetIncomingMessage message;
                while ((message = server.ReadMessage()) != null) {
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            //read
                            PacketTypes type = (PacketTypes)message.ReadByte();
                            PingPacket payPacket = PacketHelper.Get<PingPacket>(type);
                            payPacket.Deserialize(message);
                            Console.WriteLine($"We got a packet of: {type} with PingMs {payPacket.PingMs}");

                            //send
                            NetOutgoingMessage msg = server.CreateMessage();
                            PongPacket outbound = PacketHelper.Get<PongPacket>(PacketTypes.Pong);
                            outbound.PingMs = DateTime.Now.Millisecond;
                            outbound.Serialize(msg);
                            server.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                            Console.WriteLine($"We send a packet of: {PacketTypes.Pong} with PingMs {outbound.PingMs}");
             
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
