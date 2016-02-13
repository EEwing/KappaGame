using Lidgren.Network;

namespace Shared.Networking.Interfaces
{
    public abstract class Packet : IPacket
    {
        public abstract void Serialize(NetOutgoingMessage msg);
        public abstract void Deserialize(NetIncomingMessage msg);
    }
}
