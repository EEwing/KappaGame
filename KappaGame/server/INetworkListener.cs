using Lidgren.Network;
using Shared.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kappa.server {
    interface INetworkListener {
        void AcceptPacket(PacketTypes type, NetIncomingMessage message);
        void StatusChanged(NetIncomingMessage message);
    }
}
