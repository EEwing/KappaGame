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

        List<IRenderable> renderables;

        Body floor;

        public Map() {
            renderables = new List<IRenderable>();
            World = new World(new Vector2(0f, 20f));

            float h, w;
            h = ConvertUnits.ToSimUnits(KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds.Height);
            w = ConvertUnits.ToSimUnits(KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds.Width);

            floor = BodyFactory.CreateRectangle(World, w, 1, 1);
            floor.BodyType = BodyType.Static;
            floor.Position = new Vector2(w/2, h-1);
            floor.Friction = 0f;
        }

        public void LoadContent(ContentManager content) {
            texture = content.Load<Texture2D>("textures/maps/map1.png");
        }

        public override T CreateEntity<T>(T ent) {
            //public override T CreateEntity()<T> where T : Entity {
            if(ent is IRenderable)
                renderables.Add((IRenderable) ent);

            return base.CreateEntity(ent);
            //return base(ent);
        }

        public void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds, Color.White);

            // Render only entities that are renderable.
            foreach (IRenderable renderable in renderables)
                renderable.Render(spriteBatch);
            //((IRenderable)(from renderable in renderables select renderable)).Render(spriteBatch);

        }

    }
}
