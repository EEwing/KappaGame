using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Client {
    class Client : NetClient {

        private bool isListening = false;
        Thread messageThread;

        public Client(NetPeerConfiguration config) : base(config) {
            messageThread = new Thread(MessageEngine);
        }

        public void Stop() {
            isListening = false;
            messageThread.Join();
            Console.WriteLine("Client Stopped.");
        }

        public void ListenForMessages() {
            isListening = true;
            messageThread.Start();
        }

        private void MessageEngine() {
            NetIncomingMessage message;
            while (isListening) {
                while ((message = ReadMessage()) != null) {
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            Console.WriteLine("CLIENT: Read Data here");
                            //var data = message.Read * ();
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            //switch (message.SenderConnection.Status) {
                                /* .. */
                            //}
                            Console.WriteLine($"CLIENT: Status Changed = {message.SenderConnection.Status}");
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine($"CLIENT: {message.ReadString()}");
                            break;

                        /* .. */
                        case NetIncomingMessageType.WarningMessage:
                            Console.WriteLine($"CLIENT: WARNING: {message}");
                            string msg = message.ReadString();
                            Console.WriteLine(msg);
                            break;

                        default:
                            Console.WriteLine("CLIENT: unhandled message with type: "
                                + message.MessageType);
                            Console.WriteLine($"CLIENT: connectiontype: {message.SenderConnection}");
                            break;
                    }
                }
            }
        }

    }
}
