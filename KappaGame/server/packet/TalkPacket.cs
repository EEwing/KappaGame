using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kappa.server.packet {
    class TalkPacket : Packet {

        public string Message { get; set; }

    }
}
