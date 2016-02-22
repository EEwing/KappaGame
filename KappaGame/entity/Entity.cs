using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Kappa.server;
using Kappa.world;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Kappa.entity {
    abstract class Entity : Damagable, IDynamicObject, ISerializable {

        private MapModel map_;

        protected UpdateHistory UpdateHistory = new UpdateHistory();

        public Guid id;
        public MapModel Map { get { return map_; } set { map_ = value; GenerateBody(); } }
        public int NetworkUpdatePriority { get; protected set; } = 1;
        public Body body { get; protected set; }
        public bool OnFloor { get; private set; }
        public float MaxSpeed { get; set; } = 10;

        public abstract void GenerateBody();

        public Entity(Guid newID) {
            id = newID;
        }

        public Entity() : this(Guid.Empty) { }
        /*
        protected Entity(FarseerPhysics.Dynamics.World world) {
            body = BodyFactory.CreateBody(world, Vector2.Zero);
        }
        */

        public virtual void Update(float dt) {
            if (body.LinearVelocity.X > MaxSpeed) {
                body.LinearVelocity = new Vector2(MaxSpeed, body.LinearVelocity.Y);
            }
            if (body.LinearVelocity.X < -MaxSpeed) {
                body.LinearVelocity = new Vector2(-MaxSpeed, body.LinearVelocity.Y);
            }
            if (body.LinearVelocity.Y > MaxSpeed) {
                body.LinearVelocity = new Vector2(body.LinearVelocity.X, MaxSpeed);
            }
            if (body.LinearVelocity.Y < -MaxSpeed) {
                body.LinearVelocity = new Vector2(body.LinearVelocity.X, -MaxSpeed);
            }

            EntityState state = new EntityState();
            state.TimeStamp = DateTime.Now.Ticks;
            state.id = id;
            state.Velocity = body.LinearVelocity;
            state.Position = body.Position;
            UpdateHistory.AddState(state);
            //Console.WriteLine($"Player is travelling at {body.LinearVelocity.Length()} m/s");
        }

        protected virtual bool OnCollision(Fixture fix1, Fixture fix2, Contact contact) {
            return true;
        }

        public virtual void Serialize(NetOutgoingMessage message) {
            // Implement map checksum to ensure correct map
            // Add in changes to map in serialization
            message.Write(id.ToString());
            message.Write(MaxSpeed);
            message.Write(OnFloor);
            message.Write(body.LinearVelocity.X);
            message.Write(body.LinearVelocity.Y);
            message.Write(body.Mass);
            message.Write(body.Position.X);
            message.Write(body.Position.Y);
        }

        public virtual void Deserialize(NetIncomingMessage message) {
            id = Guid.Parse(message.ReadString());
            MaxSpeed = message.ReadFloat();
            OnFloor = message.ReadBoolean();
            body.LinearVelocity = new Vector2(message.ReadFloat(), message.ReadFloat());
            body.Mass = message.ReadFloat();
            body.Position = new Vector2(message.ReadFloat(), message.ReadFloat());
        }

        public EntityState GetState() {
            return UpdateHistory.GetLastState();
        }
    }
}
