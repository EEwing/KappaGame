using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kappa.gui.interaction;

namespace Kappa.gui.scenes {
    class SceneMainMenu : Scene {

        static Texture2D menu = null;
        static Texture2D button = null;
        Button b;
        
        public override void Exit() {
            
        }

        public override void LoadContent(ContentManager content) {
            if(menu == null)
                menu = content.Load<Texture2D>("textures/menu/menumain");

            if (button == null)
                button = content.Load<Texture2D>("textures/menu/main/play");
        }

        public override void Initialize() {
            int w = button.Bounds.Width * 2;
            int h = button.Bounds.Height * 2;
            Rectangle window = KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds;
            b = new Button(new Rectangle((window.Width - w)/2, (window.Height - h)/2, w, h), button);
            b.ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneInGame());
        }

        public override void Render(SpriteBatch spriteBatch) {
            Rectangle bounds = KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds;
            spriteBatch.Draw(menu, new Rectangle(0, 0, bounds.Width, bounds.Height), Color.White);
            b.Render(spriteBatch);
        }

        public override void Update(float dt) {
            Console.WriteLine("Updating SceneMainMenu");
            b.Update(dt);
        }
    }
}
