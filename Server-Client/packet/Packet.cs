using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Client.packet {
    abstract class Packet {

        public enum PacketType : byte {
            MESSAGE
        }

        public abstract PacketType Type { get; }
    }
}
