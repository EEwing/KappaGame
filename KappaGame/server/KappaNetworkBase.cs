using Kappa.world;
using Lidgren.Network;
using Shared.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kappa.server {
    abstract class KappaNetworkBase : INetworkListener {

        private Thread messageThread;

        protected SortedDictionary<Guid, long> UpdatePriorities = new SortedDictionary<Guid, long>();
        protected abstract NetPeer Peer { get; }
        protected int bitrate = 100; // in Kbps
        protected bool isListening = false;
        protected NetPeerConfiguration config;
        protected string Name = "";

        public Map Map { get; set; }

        public KappaNetworkBase(string Name = "Generic Kappa Network") {
            messageThread = new Thread(ListenForMessages);

            UpdatePriorities.OrderBy(kvp => kvp.Value);

            config = new NetPeerConfiguration("KappaGame");
            //Peer = new NetPeer(config);

            this.Name = Name;
        }
        
        public void Start() {
            // Load packets if server isn't local (in local memory)
            Peer.Start();
            isListening = true;
            messageThread.Start();
            OnStart();
        }

        public void Stop() {
            Console.WriteLine($"\n\n Stopping network object: {Name} \n");
            isListening = false;
            messageThread.Join();
            OnExit();
        }

        protected virtual void OnExit() { }
        protected virtual void OnStart() { }

        public abstract void update(float dt);

        private void ListenForMessages() {
            NetIncomingMessage message;
            Console.WriteLine($"{Name}: Listening for messages");
            while (isListening) {
                while ((message = Peer.ReadMessage()) != null) {
                    //Console.WriteLine("Reading message");
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            PacketTypes type = (PacketTypes)message.ReadByte();
                            AcceptPacket(type, message);
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            //switch (message.SenderConnection.Status) {
                            /* .. */
                            //}
                            Console.WriteLine($"{Name}: Status Changed = {message.SenderConnection.Status}");
                            StatusChanged(message);
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine($"{Name}: {message.ReadString()}");
                            break;

                        /* .. */
                        case NetIncomingMessageType.WarningMessage:
                            Console.WriteLine($"{Name}: WARNING: {message}");
                            break;

                        default:
                            Console.WriteLine($"{Name}: unhandled message with type: "
                                + message.MessageType);
                            break;
                    }
                }
            }
            Console.WriteLine($"{Name}: Done listening for messages");
        }

        public virtual void StatusChanged(NetIncomingMessage message) { }
        public abstract void AcceptPacket(PacketTypes type, NetIncomingMessage message);
    }
}
