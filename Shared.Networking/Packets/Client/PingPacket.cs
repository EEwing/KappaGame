using Lidgren.Network;
using Shared.Networking.Interfaces;

namespace Shared.Networking.Packets.Client
{
    public class PingPacket : Packet
    {
        public float PingMs { get; set; }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write((byte)PacketTypes.Ping);
            msg.Write(PingMs);
        }

        public override void Deserialize(NetIncomingMessage msg)
        {
            PingMs = msg.ReadFloat();
        }
    }
}
