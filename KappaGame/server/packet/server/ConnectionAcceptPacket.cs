using Shared.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Shared.Networking;

namespace Kappa.server.packet.server {
    class ConnectionAcceptPacket : Packet {

        public Guid ID { get; set; } = Guid.Empty;

        public override void Deserialize(NetIncomingMessage msg) {
            ID = Guid.Parse(msg.ReadString());
        }

        public override void Serialize(NetOutgoingMessage msg) {
            msg.Write((byte)PacketTypes.ConnectionAccept);
            msg.Write(ID.ToString());
        }
    }
}
