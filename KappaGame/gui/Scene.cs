using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kappa.gui.scenes;

namespace Kappa.gui {
    abstract class Scene : IRenderable, IDynamicObject {

        public static Scene MAIN_MENU = new SceneMainMenu();
        public static Scene OPTIONS = new SceneOptions();
        public static Scene IN_GAME = new SceneInGame(); // Possibly should be more verbose in scene choices....

        public abstract void LoadContent(ContentManager content);
        public abstract void Render(SpriteBatch spriteBatch);

        public abstract void Update(float dt);

        public abstract void Initialize();

        public abstract void Exit();
    }
}
