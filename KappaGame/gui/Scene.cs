using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kappa.gui {
    abstract class Scene : IRenderable, IDynamicObject {

        public abstract void LoadContent(ContentManager content);
        public abstract void Render(SpriteBatch spriteBatch);

        public abstract void Update(float dt);

        public abstract void Initialize();

        public abstract void Exit();
    }
}
