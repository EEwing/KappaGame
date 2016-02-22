using Lidgren.Network;
using Shared.Networking.Interfaces;

namespace Shared.Networking.Packets.Server
{
    public class PongPacket : Packet
    {
        public float PingMs { get; set; }
        
        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write((byte)PacketTypes.Pong);
            msg.Write(PingMs);
        }

        public override void Deserialize(NetIncomingMessage msg)
        {
            PingMs = msg.ReadFloat();
        }
    }
}
