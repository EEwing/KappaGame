using Kappa.gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics;
using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using Kappa.world;

namespace Kappa.entity {
    class Player : PlayerModel, IRenderable {

        private static Texture2D texture;
        private static Texture2D hitbox;

        public Player(Map map) : base(map) {
        }

        public void LoadContent(ContentManager content) {
            if(texture == null) 
                texture = content.Load<Texture2D>("textures/entities/player");
            if (hitbox == null)
                hitbox = content.Load<Texture2D>("textures/entities/hitbox");
        }

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, new Vector2(ConvertUnits.ToDisplayUnits(body.Position.X), ConvertUnits.ToDisplayUnits(body.Position.Y)));
            /*
            AABB box;
            body.GetAABB(out box, 0);
            Vector2 TopLeft = fixture.Body.Position;
            Vector2 BottomRight = TopLeft + new Vector2(box.Width, box.Height);
            Rectangle bbox = new Rectangle((int) ConvertUnits.ToDisplayUnits(TopLeft.X), (int) ConvertUnits.ToDisplayUnits(TopLeft.Y), 
                                            (int) ConvertUnits.ToDisplayUnits(box.Width), (int) ConvertUnits.ToDisplayUnits(box.Height));
            Console.WriteLine($"Bounding box is at: {bbox}, Center is at {box.Center}");
            //Console.WriteLine($"Body Position: {topLeft}");

            spriteBatch.Draw(hitbox, bbox, Color.White);
            //spriteBatch.Draw(texture, Location);
            */
        }

        public override void Update(float dt) {
            float force = 10000;
            if (Keyboard.GetState().IsKeyDown(Keys.W)) {
                body.ApplyLinearImpulse(new Vector2(0, -force*dt));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                body.ApplyLinearImpulse(new Vector2(-force*dt, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S)) {
                body.ApplyLinearImpulse(new Vector2(0, force*dt));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                body.ApplyLinearImpulse(new Vector2(force*dt, 0));
            }
            if(canJump && !isJumping && Keyboard.GetState().IsKeyDown(Keys.Space)) {
                body.ApplyLinearImpulse(new Vector2(0, -0.5f*body.Mass*Map.World.Gravity.Y));
                isJumping = true;
            }
        }
    }
}
