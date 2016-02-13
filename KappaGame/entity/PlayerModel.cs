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

        public override void Update(float dt) {
            base.Update(dt);
        }

        protected bool isJumping = false;
        protected bool canJump = false;

        protected PlayerModel() {
        }

        protected override bool OnCollision(Fixture fix1, Fixture fix2, Contact contact) {
            base.OnCollision(fix1, fix2, contact);

            if (contact.IsTouching && contact.Manifold.LocalNormal.Y < 0) {
                canJump = true;
                isJumping = false;
            }
            return true;
        }

        public override void GenerateBody() {
            body = BodyFactory.CreateRectangle(Map.World, 0.5f, 1f, 1);
            body.BodyType = BodyType.Dynamic;
            body.FixedRotation = true;
            body.Mass = 200;
            body.OnCollision += this.OnCollision;
        }

    }
}
