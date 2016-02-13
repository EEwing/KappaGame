using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kappa.gui.interaction;

namespace Kappa.gui.scenes {
    class SceneMainMenu : SceneButtonList {

        Texture2D menu = null;

        Button playButton = null;
        Button optionsButton = null;
        Button bitmineButton = null;
        Button exitButton = null;

        public SceneMainMenu() {



            //playButton = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 2), w, h));
            playButton = new Button();
            playButton.ButtonPressed = () => SceneController.Instance.SwitchToScene(Scene.IN_GAME);

            //bitmineButton = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 1), w, h));
            bitmineButton = new Button();
            //buttons[1].ButtonPressed = () => SceneController.Instance.SwitchToScene(new SceneInGame());

            //optionsButton = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 0), w, h));
            optionsButton = new Button();
            optionsButton.ButtonPressed = () => SceneController.Instance.SwitchToScene(Scene.OPTIONS);

            //exitButton = new Button(new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * -1), w, h));
            exitButton = new Button();
            exitButton.ButtonPressed = () => KappaGame.Instance.Quit();

        }

        public override void Exit() {
            
        }

        public override void LoadContent(ContentManager content) {
            if (menu == null)
                menu = content.Load<Texture2D>("textures/menu/menumain");

            Console.WriteLine("Loading Content for Main Menu");
            if (!playButton.HasTexture()) {
                playButton.Texture = content.Load<Texture2D>("textures/menu/main/button/play");
            }

            if (!bitmineButton.HasTexture()) { 
                bitmineButton.Texture = content.Load<Texture2D>("textures/menu/main/button/bitmine");
            }

            if (!optionsButton.HasTexture()) { 
                optionsButton.Texture = content.Load<Texture2D>("textures/menu/main/button/options");
            }

            if (!exitButton.HasTexture()) { 
                exitButton.Texture = content.Load<Texture2D>("textures/menu/main/button/exit");
            }
        }

        public override void Initialize() {

            int space = 32;

            int w = playButton.Texture.Bounds.Width;
            int h = playButton.Texture.Bounds.Height;
            Rectangle window = KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds;
            
            playButton.Bounds = new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 2), w, h);
            bitmineButton.Bounds = new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 1), w, h);
            optionsButton.Bounds = new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * 0), w, h);
            exitButton.Bounds = new Rectangle((window.Width - w) / 2, (window.Height - h) / 2 - ((h + space) * -1), w, h);

            buttons.Add(playButton);
            buttons.Add(optionsButton);
            buttons.Add(bitmineButton);
            buttons.Add(exitButton);
        }

        public override void Render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(menu, KappaGame.Instance.GraphicsDevice.PresentationParameters.Bounds,  Color.White);
            //spriteBatch.DrawString( KappaGame.Instance.FPS);
            base.Render(spriteBatch);
        }

        public override void Update(float dt) {
            base.Update(dt);
        }
    }
}
