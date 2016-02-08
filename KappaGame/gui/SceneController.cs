using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kappa.gui.scenes;

namespace Kappa.gui {
    class SceneController : IRenderable, IDynamicObject {

        private Scene currentScene;
        private static SceneController instance = null;
        private ContentManager contentManager = null;

        public ContentManager Content {
            get {
                if (contentManager == null) {
                    contentManager = new ContentManager(KappaGame.Instance.Services);
                    contentManager.RootDirectory = "Content";
                }
                return contentManager;
            }
        }

        public SceneController() {
            LoadScene(Scene.MAIN_MENU);
        }
        
        public void Render(SpriteBatch spriteBatch) {
            if(currentScene != null) {
                currentScene.Render(spriteBatch);
            }
        }

        public void LoadContent(ContentManager content) {
            throw new NotImplementedException();
        }

        public void UnloadContent() {
            Content.Unload();
        }

        public static SceneController Instance { get {
                if (instance == null)
                    instance = new SceneController();

                return instance;
        } }

        public void SwitchToScene(Scene newScene, bool transition=false) {
            if(currentScene != null) {
                currentScene.Exit();
            }
            LoadScene(newScene);
        }

        public void Update(float dt) {
            if (currentScene != null)
                currentScene.Update(dt);
        }

        private void LoadScene(Scene scene) {
            currentScene = scene;
            currentScene.LoadContent(Content);
            currentScene.Initialize();
        }
    }
}
