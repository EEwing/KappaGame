using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kappa.world;
using Kappa.entity;

namespace Kappa.gui.scenes {
    class SceneInGame : Scene {

        Map map;

        public SceneInGame() {
            map = new Map();
        }

        public override void Exit() {
        }

        public override void Initialize() {
        }

        public override void LoadContent(ContentManager content) {
            map.LoadContent(content);
        }

        public override void Render(SpriteBatch spriteBatch) {
            map.Render(spriteBatch);
        }

        public override void Update(float dt) {
            map.Update(dt);
        }
    }
}
