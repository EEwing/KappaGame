using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
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
    abstract class PlayerModel : Entity {

        public abstract override void Update(float dt);

        protected bool isJumping = false;
        protected bool canJump = true;

        protected PlayerModel(Map map) {
            body = BodyFactory.CreateRectangle(map.World, 0.5f, 1f, 1);
            //Body body = BodyFactory.CreateBody(world, Vector2.Zero);
            body.BodyType = BodyType.Dynamic;
            body.FixedRotation = true;
            body.Mass = 200;
            body.OnCollision += this.OnCollision;
            Map = map;
            //Shape shape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 1f), 1);
            //fixture = body.CreateFixture(shape);
        }

        protected override bool OnCollision(Fixture fix1, Fixture fix2, Contact contact) {
            if (contact.IsTouching && contact.Manifold.LocalNormal.Y > 0) {
                Console.WriteLine("Player can jump now!");
                canJump = true;
                isJumping = false;
            }

            return true;
        }
    }
}
