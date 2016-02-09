using Kappa.server.packet;
using Network;
using Network.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kappa.server {
    class OnlineServer : Server {

        ServerConnectionContainer connections;

        public override void Run() {
            ConnectionFactory.AddKnownTypes(typeof(TalkPacket));
            connections = ConnectionFactory.CreateServerConnectionContainer(ConnectionPort);

            connections.ConnectionLost += (a, b, c) => Console.WriteLine($"{b.ToString()} Connection lost {a.IPRemoteEndPoint.Port}. Reason {c.ToString()}");
            connections.ConnectionEstablished += connectionEstablished;
        }

        private void connectionEstablished(Connection connection, ConnectionType type) {
            Console.WriteLine("Connection established!");
            Console.WriteLine($"{connection.GetType()} connected on port {connection.IPLocalEndPoint.Port}");
            connection.RegisterPacketHandler(typeof(TalkPacket), messageReceived);
        }

        private void messageReceived(Packet packet, Connection connection) {
            TalkPacket request = (TalkPacket)packet;
            TalkPacket response = new TalkPacket();
            response.Message = "Message returning from server: " + request.Message;
            connection.Send(response);
        }
    }
}
