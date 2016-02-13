using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Client {
    class Program {
        
        static void Main(string[] args) {
            string appName = "Server Client Test";

            NetPeerConfiguration serverConfig = new NetPeerConfiguration(appName) { Port = 8080 };
            var server = new Server(serverConfig);
            server.Start();
            Console.WriteLine($"Server started on port {server.Port}. Status: {server.Status}");
            server.ListenForMessages();

            NetPeerConfiguration clientConfig = new NetPeerConfiguration(appName);
            var client = new Client(clientConfig);
            client.Start();
            //client.DiscoverLocalPeers(8080);
            client.Connect(host: "127.0.0.1", port: server.Port);
            while (client.ServerConnection == null) { }

            Console.WriteLine(client.ServerConnection.Status);
            client.ListenForMessages();

            var Message = client.CreateMessage();
            Message.Write("Hello!");
            Console.WriteLine("Sending message");
            NetSendResult sendResult = client.SendMessage(Message, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine(sendResult);

            Thread.Sleep(1000);
            client.Stop();
            server.Stop();
        }
    }
}
