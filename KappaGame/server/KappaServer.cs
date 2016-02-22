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
    class KappaServer : KappaNetworkBase, INetworkListener {
        PlayerManager pManager = new PlayerManager();
        
        public int ConnectionPort { get { return config.Port; } }

        private Thread updateThread;
        private NetServer _server;

        protected override NetPeer Peer {
            get {
                if(_server == null) {
                    config.Port = 12424;
                    _server = new NetServer(config);
                    return _server;
                }
                return _server;
            }
        }

        private void UpdateEngine() {
            DateTime last = DateTime.Now;
            while (isListening) {
                //Console.Write($"IS LISTENING? {isListening}");
                DateTime cur = DateTime.Now;
                TimeSpan dtSpan = cur.Subtract(last);
                last = cur;

                float dt = (float)dtSpan.TotalMilliseconds;

                lock(Map) {
                    Map.Update(dt);
                }

                update(dt);
            }
        }

        public KappaServer(List<PlayerModel> playerList = null) : base("SERVER") {
            Map = new Map();
            updateThread = new Thread(UpdateEngine);
            config.Port = 42069;
            if (playerList != null)
                AddPlayers(playerList);
        }

        public void AddPlayers(List<PlayerModel> playerList) {
            //pManager.Players.Add((PlayerModel) (from p in playerList select p));
            foreach (Player p in playerList) {
                pManager.Players.Add(p);
            }
        }

        protected override void OnStart() {
            PacketHelper.Add(PacketTypes.Ping,             typeof(PingPacket));
            PacketHelper.Add(PacketTypes.Pong,             typeof(PongPacket));
            PacketHelper.Add(PacketTypes.ConnectionAccept, typeof(ConnectionAcceptPacket));
            PacketHelper.Add(PacketTypes.MapState,         typeof(MapStatePacket));
            PacketHelper.Add(PacketTypes.EntityState,      typeof(EntityStatePacket));
            updateThread.Start();
        }

        protected override void OnExit() {
            base.OnExit();
            updateThread.Join();
        }

        public override void update(float dt) {
            //Console.WriteLine("Updated in {0} milliseconds", dt);
            foreach (Guid id in pManager.PlayerConnections.Keys) {
                Entity ent = Map.GetEntity(id);
            }

            long bytes = (long)(this.bitrate * dt / 8);
            //Console.WriteLine($"Can send {bytes} bytes");
            foreach(Guid id in UpdatePriorities.Keys.ToList()) {
                Entity ent = Map.GetEntity(id);
                if (ent != null) {
                    UpdatePriorities[id] += ent.NetworkUpdatePriority;
                    //Console.WriteLine("Priority: " + UpdatePriorities[id]);
                }
            }

            if (_server.ConnectionsCount > 0) {
                EntityStatePacket packet = new EntityStatePacket();
                packet.MaxSizeBytes = bitrate;

                var ordered = UpdatePriorities.OrderBy(kvp => kvp.Value);
                foreach (var kvp in ordered) {
                    Entity ent = Map.GetEntity(kvp.Key);
                    EntityState state = ent.GetState();
                    if (state != null)
                        packet.AddEntity(state);
                }
                var Message = _server.CreateMessage();
                packet.Serialize(Message);
                foreach (NetConnection connection in _server.Connections)
                    _server.SendMessage(Message, connection, NetDeliveryMethod.ReliableUnordered);
            }
            Thread.Sleep(10);
        }

        public override void AcceptPacket(PacketTypes type, NetIncomingMessage message) {
            switch (type) {
                case PacketTypes.Pong:
                    break;
                case PacketTypes.Ping:
                    //read
                    PingPacket payPacket = PacketHelper.Get<PingPacket>(PacketTypes.Ping);
                    payPacket.Deserialize(message);
                    Console.WriteLine($"We got a packet of: {type} with PingMs {payPacket.PingMs}");

                    //send
                    NetOutgoingMessage msg = Peer.CreateMessage();
                    PongPacket outbound = PacketHelper.Get<PongPacket>(PacketTypes.Pong);
                    outbound.PingMs = DateTime.Now.Millisecond;
                    outbound.Serialize(msg);
                    Peer.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                    Console.WriteLine($"We send a packet of: {PacketTypes.Pong} with PingMs {outbound.PingMs}");
                    break;
                case PacketTypes.EntityState:
                    EntityStatePacket statePacket = PacketHelper.Get<EntityStatePacket>(PacketTypes.EntityState);
                    statePacket.Deserialize(message);
                    foreach(EntityState state in statePacket.States) {
                        Entity ent;
                        lock(Map) {
                            ent = Map.GetEntity(state.id);
                        }
                        ent.body.LinearVelocity = state.Velocity;
                        ent.body.Position = state.Position;

                        if(state.Position.X < 0) {
                            EntityStatePacket packet = new EntityStatePacket();
                            packet.MaxSizeBytes = bitrate;
                            packet.AddEntity(ent.GetState());
                            var Message = _server.CreateMessage();
                            packet.Serialize(Message);
                        }
                        //Console.WriteLine($"Updated by network: {ent.id.ToString()} - {ent.body.Position}, {ent.body.LinearVelocity}");
                    }
                    break;
                default:
                    break;
            }
        }

        public override void StatusChanged(NetIncomingMessage message) {
            if(message.SenderConnection.Status == NetConnectionStatus.Connected) {
                // Send connection its Guid
                ConnectionAcceptPacket packet = PacketHelper.Get<ConnectionAcceptPacket>(PacketTypes.ConnectionAccept);
                packet.ID = Guid.NewGuid();
                var toSend = _server.CreateMessage();
                packet.Serialize(toSend);
                _server.SendMessage(toSend, message.SenderConnection, NetDeliveryMethod.ReliableUnordered);

                MapStatePacket mapState = PacketHelper.Get<MapStatePacket>(PacketTypes.MapState);
                mapState.Map = this.Map;
                var msg = _server.CreateMessage();
                mapState.Serialize(msg);
                _server.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableUnordered);

                UpdatePriorities.Add(packet.ID, 0);
                lock(Map) {
                    Player p = Map.CreateEntity(new Player(packet.ID));
                }
                Console.WriteLine($"Entity Created: {packet.ID}");
            }
        }
    }
}
