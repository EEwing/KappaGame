using Shared.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Kappa.entity;
using Shared.Networking;
using Kappa.world;
using Microsoft.Xna.Framework;

namespace Kappa.server.packet {
    class EntityStatePacket : Packet {

        public int MaxSizeBytes { get; set; } = 0;
        private int numEnts_;
        public List<EntityState> States { get; private set; } = new List<EntityState>();
        private readonly int headersize = sizeof(PacketTypes) + sizeof(int);

        NetBuffer finalBuffer = new NetBuffer(); 

        public override void Deserialize(NetIncomingMessage msg) {
            numEnts_ = msg.ReadInt32();
            for(int i=0; i<numEnts_; ++i) {
                EntityState state = new EntityState();
                state.id = Guid.Parse(msg.ReadString());
                //Console.WriteLine("Reading id :" + state.id.ToString());

                Vector2 vec = new Vector2(msg.ReadFloat(), msg.ReadFloat());
                state.Position = vec;

                vec = new Vector2(msg.ReadFloat(), msg.ReadFloat());
                state.Velocity = vec;
                
                States.Add(state);  // Weird...
            }
        }

        public override void Serialize(NetOutgoingMessage msg) {
            msg.Write((byte)PacketTypes.EntityState);
            msg.Write(numEnts_);
            if(numEnts_ != 0)
                msg.Write(finalBuffer);
        }

        public void AddEntity(EntityState state) {
            NetBuffer tempBuffer = new NetBuffer();
            tempBuffer.Write(state.id.ToString());
            tempBuffer.Write(state.Position.X);
            tempBuffer.Write(state.Position.Y);
            tempBuffer.Write(state.Velocity.X);
            tempBuffer.Write(state.Velocity.Y);
            if(finalBuffer.LengthBytes + tempBuffer.LengthBytes <= MaxSizeBytes - headersize) {
                finalBuffer.Write(tempBuffer);
                ++numEnts_;
            }
        }
    }
}
