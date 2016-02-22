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
using Lidgren.Network;

namespace Kappa.entity {
    abstract class PlayerModel : Entity {

        public override void Update(float dt) {
            base.Update(dt);
        }

        protected bool isJumping = false;
        protected bool canJump = false;

        public PlayerModel() : this(Guid.Empty) { }

        public PlayerModel(Guid id) : base(id) {
            NetworkUpdatePriority = 1000000;
            UpdateHistory = new UpdateHistory();
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
            body = BodyFactory.CreateRectangle(Map.World, .5f, .5f, 1);
            body.BodyType = BodyType.Dynamic;
            body.FixedRotation = true;
            body.Mass = 200;
            body.OnCollision += this.OnCollision;
        }

        public override void Serialize(NetOutgoingMessage message) {
            base.Serialize(message);
            message.Write(isJumping);
            message.Write(canJump);
        }
        

        public override void Deserialize(NetIncomingMessage message) {
            base.Deserialize(message);
            isJumping = message.ReadBoolean();
            canJump = message.ReadBoolean();
        }

    }
}
