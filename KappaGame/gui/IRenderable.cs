using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.gui {
    interface IRenderable {
        void LoadContent(ContentManager content);
        void Render(SpriteBatch spriteBatch);
    }
}
