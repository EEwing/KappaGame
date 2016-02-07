using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kappa.gui.interaction;

namespace Kappa.gui.scenes {
    class SceneMainMenu : SceneGui {

        Texture2D menu = null;
        Texture2D buttonPlayTX = null;
        Texture2D buttonBitmineTX = null;
        Texture2D buttonOptionsTX = null;
        Texture2D buttonExitTX = null;

        public override void Exit() {
            
        }

        public override void LoadContent(ContentManager content) {
            if(menu == null)
                menu = content.Load<Texture2D>("textures/menu/menumain");

            if (buttonPlayTX == null)
                buttonPlayTX = content.Load<Texture2D>("textures/menu/main/button/play");

            if (buttonBitmineTX == null)
                buttonBitmineTX = content.Load<Texture2D>("textures/menu/main/button/bitmine");

            if (buttonOptionsTX == null)
                buttonOptionsTX = content.Load<Texture2D>("textures/menu/main/button/options");

            if (buttonExitTX == null)
                buttonExitTX = content.Load<Texture2D>("textures/menu/main/button/exit");
        }


        public override void Initialize() {
            int w = buttonPlayTX.Bounds.Width * 1;
            int h = buttonPlayTX.Bounds.Height * 1;
            Rectangle window = KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds;

            int space = 32;

            buttons[0] = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 2), w, h), buttonPlayTX);
            buttons[0].ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneInGame());

            buttons[1] = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 1), w, h), buttonBitmineTX);
            //buttons[1].ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneInGame());

            buttons[2] = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 0), w, h), buttonOptionsTX);
            buttons[2].ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneOptions());

            buttons[3] = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * -1), w, h), buttonExitTX);
            //buttons[3].ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneInGame());
        }

        public override void RenderGui(SpriteBatch spriteBatch) {
            Rectangle bounds = KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds;
            spriteBatch.Draw(menu, new Rectangle(0, 0, bounds.Width, bounds.Height), Color.White);
        }

        public override void UpdateGui(float dt) {
            Console.WriteLine("Updating SceneMainMenu");
        }
    }
}
