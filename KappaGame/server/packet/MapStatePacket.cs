using Shared.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Kappa.world;
using Shared.Networking;

namespace Kappa.server.packet {
    class MapStatePacket : Packet {

        public Map Map { get; set; }

        public override void Deserialize(NetIncomingMessage msg) {
            Map = new Map();
        }

        public override void Serialize(NetOutgoingMessage msg) {
            msg.Write((byte)PacketTypes.MapState);
        }
    }
}
