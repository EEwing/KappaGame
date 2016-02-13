using Lidgren.Network;

namespace Shared.Networking.Interfaces
{
    public interface IPacket
    {
        void Serialize(NetOutgoingMessage msg);
        void Deserialize(NetIncomingMessage msg);
    }
}