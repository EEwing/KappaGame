using Kappa.gui.interaction;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.gui.scenes
{
    abstract class SceneGui : Scene
    {
        protected Button[] buttons = new Button[2048];

        public override void Render(SpriteBatch spriteBatch)
        {
            RenderGui(spriteBatch);
            foreach (Button button in buttons)
            {
                if (button == null) { continue; }
                button.Render(spriteBatch);
            }
        }

        public override void Update(float dt)
        {
            UpdateGui(dt);
            foreach (Button button in buttons)
            {
                if (button == null) { continue; }
                button.Update(dt);
            }
        }

        public abstract void RenderGui(SpriteBatch spriteBatch);

        public abstract void UpdateGui(float dt);

    }
}
