using Kappa.gui;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Kappa.entity;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Kappa.world {
    class Map : MapModel, IRenderable {
        Texture2D texture;
        Player player;
        Body floor;

        public Map() {
            World = new World(new Vector2(0f, 20f));
            player = new Player(this);
            //floor = BodyFactory.CreateBody(world);
            float h, w;
            h = ConvertUnits.ToSimUnits(KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds.Height);
            w = ConvertUnits.ToSimUnits(KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds.Width);
            //Shape floorShape = new PolygonShape(PolygonTools.CreateRectangle(w/2, 0.5f), 1);
            floor = BodyFactory.CreateRectangle(World, w, 1, 1);
            floor.BodyType = BodyType.Static;
            floor.Position = new Vector2(w/2, h-1);
            floor.Friction = 0f;
            //Shape floorShape = new EdgeShape(new Vector2(0, h), new Vector2(w, h));
            //Fixture fInteract = floor.CreateFixture(floorShape);
            //fInteract.Body.Position = new Vector2(0, h);
        }

        public void LoadContent(ContentManager content) {
            texture = content.Load<Texture2D>("textures/maps/map1.png");
            player.LoadContent(content);
        }

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds, Color.White);
            player.Render(spriteBatch);
        }

        public override void Update(float dt) {
            player.Update(dt);
            World.Step(dt);
        }
    }
}
