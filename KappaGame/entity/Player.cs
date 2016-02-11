using Kappa.gui;
using Kappa.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using System.Diagnostics;

namespace Kappa.entity {
    class Player : PlayerModel, IRenderable {

        private static Texture2D texture;
        private static Texture2D hitbox;

        public Player() {

        }

        public void LoadContent(ContentManager content) {
            if(texture == null) 
                texture = content.Load<Texture2D>("textures/entities/player");
            if (hitbox == null)
                hitbox = content.Load<Texture2D>("textures/entities/hitbox");
        }

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, new Vector2(ConvertUnits.ToDisplayUnits(body.Position.X), ConvertUnits.ToDisplayUnits(body.Position.Y)));
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
                float jumpVel = 10f;
                Vector2 vel = new Vector2(body.LinearVelocity.X, -jumpVel);
                //body.ApplyLinearImpulse(new Vector2(0, -0.5f*body.Mass*Map.World.Gravity.Y));
                body.LinearVelocity = vel;
                isJumping = true;
            }

            // Pathfinder Tests 
            /*
                if(Keyboard.GetState().IsKeyDown(Keys.P)) {
                    Pathfinder P = new Pathfinder(Pathfinder.GenerateTestList(100));
                    Pathfinder.PathfinderNode Start = P.GetNodeAt(0, 0);
                    Pathfinder.PathfinderNode End = P.GetNodeAt(20, 20);
                    List<Pathfinder.PathfinderNode> Path = P.FindPath(Start, End, true);

                    // Outputting the path.
                    Debug.WriteLine("Starting from (" + Start.X + ", " + Start.Y + ") and going to (" + End.X + ", " + End.Y + ")");
                    for(var i = 0; i < Path.Count; i++) {
                        Debug.WriteLine("Now at : (" + Path[i].X + ", " + Path[i].Y + ")");
                    }
                }
            */

            base.Update(dt);
        }

    }
}
