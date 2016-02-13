using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Kappa.world;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.entity {
    abstract class Entity : Damagable, IDynamicObject {

        private MapModel map_;
        public MapModel Map { get { return map_; } set { map_ = value; GenerateBody(); } }

        public Body body { get; protected set; }

        public bool OnFloor { get; private set; }

        public float MaxSpeed { get; set; } = 10;

        public abstract void GenerateBody();

        /*
        protected Entity(FarseerPhysics.Dynamics.World world) {
            body = BodyFactory.CreateBody(world, Vector2.Zero);
        }
        */

        public virtual void Update(float dt) {
            if(body.LinearVelocity.X > MaxSpeed) {
                body.LinearVelocity = new Vector2(MaxSpeed, body.LinearVelocity.Y);
            }
            if(body.LinearVelocity.X < -MaxSpeed) {
                body.LinearVelocity = new Vector2(-MaxSpeed, body.LinearVelocity.Y);
            }
            if(body.LinearVelocity.Y > MaxSpeed) {
                body.LinearVelocity = new Vector2(body.LinearVelocity.X, MaxSpeed);
            }
            if(body.LinearVelocity.Y < -MaxSpeed) {
                body.LinearVelocity = new Vector2(body.LinearVelocity.X, -MaxSpeed);
            }
            //Console.WriteLine($"Player is travelling at {body.LinearVelocity.Length()} m/s");
        }

        protected virtual bool OnCollision(Fixture fix1, Fixture fix2, Contact contact) {
            return true;
        }
    }
}
