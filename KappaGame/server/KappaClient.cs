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
using Kappa.server.packet;
using Kappa.server.packet.server;
using Kappa.world;

namespace Kappa.server
{
    class KappaClient : KappaNetworkBase {

        public int ConnectionPort { get; set; } // Maybe should go in online server...
        public Player Player { get; set; }
        public bool isConnected { get; private set; } = false;

        private NetClient _client;

        protected override NetPeer Peer { get { return _client ?? (_client = new NetClient(config)); } }

        public KappaClient() : base("CLIENT") { }

        protected override void OnExit() {
            Peer.Shutdown("Client has stopped");
        }

        // Blocking function! returns once connection is established
        public void ConnectToServer(string host, int port) {
            _client.Connect(host: host, port: port);
            while (!isConnected) { }
        }

        // Dummy function to test connection to server
        public void SendString (string toSend) {
            var Message = _client.CreateMessage();
            Message.Write(toSend);
            _client.SendMessage(Message, NetDeliveryMethod.ReliableOrdered);
        }

        public override void AcceptPacket(PacketTypes type, NetIncomingMessage message) {
            switch (type) {
                case PacketTypes.Pong:
                    //read
                    PongPacket payPacket = PacketHelper.Get<PongPacket>(type);
                    payPacket.Deserialize(message);
                    Console.WriteLine($"We got a packet of: {type} with PingMs {payPacket.PingMs}");

                    //send
                    NetOutgoingMessage msg = _client.CreateMessage();
                    PingPacket outbound = PacketHelper.Get<PingPacket>(PacketTypes.Ping);
                    outbound.PingMs = DateTime.Now.Millisecond;
                    outbound.Serialize(msg);
                    _client.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                    Console.WriteLine($"We sent a packet of: {PacketTypes.Ping} with PingMs {outbound.PingMs}");
                    break;
                case PacketTypes.Ping:
                    break;
                case PacketTypes.ConnectionAccept:
                    ConnectionAcceptPacket acceptPacket = PacketHelper.Get<ConnectionAcceptPacket>(PacketTypes.ConnectionAccept);
                    acceptPacket.Deserialize(message);
                    if(acceptPacket.ID != Guid.Empty) {
                        Console.WriteLine($"Client accepted by server! {acceptPacket.ID}");
                        Player.id = acceptPacket.ID;
                        isConnected = true;
                    }
                    break;
                case PacketTypes.MapState:
                    MapStatePacket mapState = PacketHelper.Get<MapStatePacket>(PacketTypes.MapState);
                    mapState.Deserialize(message);
                    Console.WriteLine("Received map from server");
                    break;
                case PacketTypes.EntityState:
                    EntityStatePacket statePacket = PacketHelper.Get<EntityStatePacket>(PacketTypes.EntityState);
                    statePacket.Deserialize(message);
                    foreach (EntityState state in statePacket.States) {
                        Entity ent = Map.GetEntity(state.id);
                        ent.body.LinearVelocity = state.Velocity;
                        ent.body.Position = state.Position;
                        //Console.WriteLine($"Updated by server: {ent.id.ToString()} - {ent.body.Position}, {ent.body.LinearVelocity}");
                    }
                    break;
                default:
                    break;
            }
        }

        public override void update(float dt) {
            foreach (Entity ent in Map.entities) {
                EntityStatePacket packet = new EntityStatePacket();
                packet.MaxSizeBytes = bitrate;
                packet.AddEntity(ent.GetState());
                var Message = _client.CreateMessage();
                packet.Serialize(Message);
                _client.SendMessage(Message, NetDeliveryMethod.ReliableUnordered);
            }
        }
    }
}
