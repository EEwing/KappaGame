using Kappa.gui.interaction;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.gui.scenes
{
    abstract class SceneButtonList : Scene
    {
        protected List<Button> buttons = new List<Button>();

        public override void Render(SpriteBatch spriteBatch)
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                    button.Render(spriteBatch);
            }
        }

        public override void Update(float dt)
        {
            foreach (Button button in buttons)
            {
                if (button != null) {
                    button.Update(dt);
                }
            }
        }
    }
}
