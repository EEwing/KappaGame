using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Kappa.world;
using Kappa.entity;
using Kappa.server;
using Network;
using Kappa.server.packet;
using Network.Enums;

namespace Kappa.gui.scenes {
    class SceneInGame : Scene {

        Map map;
        Player player;
        ClientConnectionContainer clientConnectionContainer;

        public SceneInGame() {
            //Server server = new OfflineServer();
            map = new Map();
            player = new Player();
            player = map.CreateEntity(player);

            List<PlayerModel> players = new List<PlayerModel>();
            players.Add(player);
            /*

            server.AddPlayers(players);
            server.Run();

            ConnectionFactory.AddKnownTypes(typeof(TalkPacket)); //ToDo: Remove after update.
            clientConnectionContainer = ConnectionFactory.CreateClientConnectionContainer("127.0.0.1", server.ConnectionPort);
            clientConnectionContainer.RegisterPacketHandler(typeof(TalkPacket), messageReceived);
            clientConnectionContainer.ConnectionEstablished += connectionEstablished;
            */    
        }

        private void connectionEstablished(Connection connection, ConnectionType connectionType) {
            TalkPacket talkPacket = new TalkPacket();
            talkPacket.Message = "Hello World!";

            Console.WriteLine("Client: Connection Established!");
            connection.Send(talkPacket);
        }

        private void messageReceived(Packet packet, Connection connection) {
            TalkPacket response = (TalkPacket)packet;
            Console.WriteLine($"Client: Message received! {response.Message}");
        }

        public override void Exit() {
        }

        public override void Initialize() {
        }

        public override void LoadContent(ContentManager content) {
            map.LoadContent(content);
            player.LoadContent(content);
        }

        public override void Render(SpriteBatch spriteBatch) {
            map.Render(spriteBatch);
        }

        public override void Update(float dt) {
            map.Update(dt);
        }
    }
}
