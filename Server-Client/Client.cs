using Network;
using Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Client {
    class Client {

        ClientConnectionContainer clientConnectionContainer;

        public Client() {
            ConnectionFactory.AddKnownTypes(typeof(TestPacket)); //ToDo: Remove after update.
            clientConnectionContainer = ConnectionFactory.CreateClientConnectionContainer("127.0.0.1", 25565);
            clientConnectionContainer.RegisterPacketHandler(typeof(TestPacket), messageReceived);
            clientConnectionContainer.ConnectionEstablished += connectionEstablished;
        }

        private void connectionEstablished(Connection connection, ConnectionType connectionType) {
            TestPacket talkPacket = new TestPacket();
            talkPacket.Message = "Hello World!";

            Console.WriteLine("Client: Connection Established!");
            connection.Send(talkPacket);
        }

        private void messageReceived(Packet packet, Connection connection) {
            TestPacket response = (TestPacket)packet;
            Console.WriteLine($"Client: Message received! {response.Message}");
        }

    }
}
