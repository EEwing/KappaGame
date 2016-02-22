using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kappa.world;
using Kappa.entity;
using Kappa.server;

namespace Kappa.gui.scenes {
    class SceneInGame: Scene {

        Map Map;
        Player player;
        KappaServer server;
        KappaClient client;
        private Camera _camera;

        public SceneInGame() {
            Map = new Map();
            player = new Player();
         }

        public override void Exit() {
            server.Stop();
            client.Stop();
        }

        public override void Initialize() {
            server = new KappaServer();
            client = new KappaClient();
            client.Map = Map;
            client.Player = player;
            player = Map.CreateEntity(player);

            List<PlayerModel> players = new List<PlayerModel>();
            players.Add(player);

            server.Start();
            client.Start();

            client.ConnectToServer("127.0.0.1", server.ConnectionPort);

            client.SendString("We're connected, baby!");

            _camera = new Camera(KappaGame.Instance.GraphicsDevice.Viewport, 1024, 1024, 1, player);
        }

        public override void LoadContent(ContentManager content) {
            Map.LoadContent(content);
            player.LoadContent(content);
        }

        public override void Render(SpriteBatch spriteBatch) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, _camera.GetMatrix());
            Map.Render(spriteBatch);
        }

        public override void Update(float dt) {
            _camera.Update();
            Map.Update(dt);
            client.update(dt);
        }
    }
}
