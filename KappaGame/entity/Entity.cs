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

        public Map Map { get; protected set; }

        public Body body { get; protected set; }

        /*
        protected Entity(FarseerPhysics.Dynamics.World world) {
            body = BodyFactory.CreateBody(world, Vector2.Zero);
        }
        */

        public abstract void Update(float dt);

        protected abstract bool OnCollision(Fixture fix1, Fixture fix2, Contact contact);
    }
}
