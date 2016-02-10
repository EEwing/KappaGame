using Network;
using Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Client {
    class Server {

        ServerConnectionContainer connections;

        public void Start() {
            ConnectionFactory.AddKnownTypes(typeof(TestPacket));
            connections = ConnectionFactory.CreateServerConnectionContainer(25565);

            connections.ConnectionLost += (a, b, c) => Console.WriteLine($"{b.ToString()} Connection lost {a.IPRemoteEndPoint.Port}. Reason {c.ToString()}");
            connections.ConnectionEstablished += connectionEstablished;

            connections.Start();

        }
        private void connectionEstablished(Connection connection, ConnectionType type) {
            Console.WriteLine("Connection established!");
            Console.WriteLine($"{connection.GetType()} connected on port {connection.IPLocalEndPoint.Port}");
            connection.RegisterPacketHandler(typeof(TestPacket), messageReceived);
        }

        private void messageReceived(Packet packet, Connection connection) {
            TestPacket request = (TestPacket)packet;
            TestPacket response = new TestPacket();
            response.Message = "Message returning from server: " + request.Message;
            connection.Send(response);
        }
    }
}
