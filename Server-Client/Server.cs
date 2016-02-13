﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Client {
    class Server : NetServer {

        Thread messageThread;
        bool isListening = false;

        public Server(NetPeerConfiguration config) : base(config) {
            messageThread = new Thread(MessageEngine);
        }

        public void ListenForMessages() {
            isListening = true;
            messageThread.Start();
        }

        public void Stop() {
            Console.WriteLine("Stopping Server");
            isListening = false;
            messageThread.Join();
            Console.WriteLine("Server Stopped.");
        }

        private void MessageEngine() {
            NetIncomingMessage message;
            Console.WriteLine("SERVER: Listening for messages");
            while (isListening) {
                while ((message = ReadMessage()) != null) {
                    switch (message.MessageType) {
                        case NetIncomingMessageType.Data:
                            // handle custom messages
                            Console.WriteLine("SERVER: Read Data here");
                            Console.WriteLine(message.ReadString());
                            //var data = message.Read * ();
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            // handle connection status messages
                            //switch (message.SenderConnection.Status) {
                                /* .. */
                            //}
                            Console.WriteLine($"SERVER: Status Changed = {message.SenderConnection.Status}");
                            break;

                        case NetIncomingMessageType.DebugMessage:
                            // handle debug messages
                            // (only received when compiled in DEBUG mode)
                            Console.WriteLine($"SERVER: {message.ReadString()}");
                            break;

                        /* .. */
                        case NetIncomingMessageType.WarningMessage:
                            Console.WriteLine($"SERVER: WARNING: {message}");
                            break;

                        default:
                            Console.WriteLine("SERVER: unhandled message with type: "
                                + message.MessageType);
                            break;
                    }
                }
            }
            Console.WriteLine("SERVER: Done listening for messages");
        }
    }
}
