#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : Network
// Author           : Thomas
// Created          : 07-23-2015
//
// Last Modified By : Thomas
// Last Modified On : 08-05-2015
// ***********************************************************************
// <copyright>
// Company: Indie-Dev
// Thomas Christof (c) 2015
// </copyright>
// <License>
// GNU LESSER GENERAL PUBLIC LICENSE
// </License>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// ***********************************************************************
#endregion Licence - LGPLv3
using Network.Attributes;
using Network.Enums;
using Network.Extensions;
using Network.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Network
{
    /// <summary>
    /// If this instance received a packet, this delegate will be used to deliver the packet and
    /// the receiving network instance.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="connection">The connection.</param>
    public delegate void PacketReceivedHandler(Packet packet, Connection connection);

    /// <summary>
    /// Provides the basic methods a connection has to implement.
    /// It ensures the connectivity, is able to send pings and keeps track of the latency.
    /// Every connection instance has 3 threads:
    /// - (1) Send thread       -> Writes enqueued packets on the stream
    /// - (2) Read thread       -> Read bytes from the stream
    /// - (3) Invoke thread     -> Delegates the received packets to the given delegate.
    /// All 3 threads will be automatically aborted if the connection has been closed.
    /// After closing the connection, every packet in the send queue will be send before closing the connection.
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        /// Constants.
        /// </summary>
        private const int PING_INTERVALL = 5000;
        protected const int CPU_SAVE = 5;

        /// <summary>
        /// True if this instance should send in a specific interval a keep alive packet, to ensure
        /// whether there is a connection or not. If set to [false] <see cref="RTT"/> and <see cref="Ping"/> wont be enabled/refreshed.
        /// </summary>
        private bool keepAlive = false;

        /// <summary>
        /// A fix hashCode that does not change, even if the most objects changed their values.
        /// </summary>
        private int hashCode;

        /// <summary>
        /// A handler which will be invoked if this connection is dead.
        /// </summary>
        private event Action<CloseReason, Connection> connectionClosed;
        private event Action<TcpConnection, UdpConnection> connectionEstablished;
        private ConcurrentQueue<UdpConnection> pendingUDPConnections = new ConcurrentQueue<UdpConnection>();

        /// <summary>
        /// When this stopwatch reached the <see cref="TIMEOUT"/> the instance is going to send a ping request.
        /// </summary>
        private Stopwatch nextPingStopWatch = new Stopwatch();
        private Stopwatch currentPingStopWatch = new Stopwatch();

        /// <summary>
        /// This concurrent queue contains the received/send packets which we have to handle.
        /// </summary>
        private ConcurrentQueue<Packet> receivedPackets = new ConcurrentQueue<Packet>();
        private ConcurrentQueue<Packet> sendPackets = new ConcurrentQueue<Packet>();

        #region Threads
        private Thread readStreamThread;
        private Thread writeStreamThread;
        private Thread invokePacketThread;
        #endregion Threads

        /// <summary>
        /// Maps the type of a packet to their byte value.
        /// </summary>
        private static BiDictionary<Type, ushort> typeByte = new BiDictionary<Type, ushort>();

        /// <summary>
        /// Maps the types of the packets to the responsible handler.
        /// </summary>
        private Dictionary<Type, PacketReceivedHandler> packetHandler = new Dictionary<Type, PacketReceivedHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        internal Connection()
        {
            //Set the hashCode of this instance.
            hashCode = this.GenerateUniqueHashCode();
        }

        /// <summary>
        /// Initializes the specified connection stream.
        /// </summary>
        /// <param name="connectionStream">The connection stream.</param>
        /// <param name="endPoint">The end point.</param>
        internal void Init()
        {
            readStreamThread = new Thread(ReadWork);
            readStreamThread.Priority = ThreadPriority.Normal;
            readStreamThread.Name = $"Read Thread {IPLocalEndPoint.AddressFamily.ToString()}";

            writeStreamThread = new Thread(WriteWork);
            writeStreamThread.Priority = ThreadPriority.Normal;
            writeStreamThread.Name = $"Write Thread  {IPLocalEndPoint.AddressFamily.ToString()}";

            invokePacketThread = new Thread(InvokeWork);
            invokePacketThread.Priority = ThreadPriority.Normal;
            invokePacketThread.Name = $"Invoke Thread  {IPLocalEndPoint.AddressFamily.ToString()}";

            readStreamThread.Start();
            writeStreamThread.Start();
            invokePacketThread.Start();
        }

        /// <summary>
        /// Initializes static members of the <see cref="Connection"/> class.
        /// </summary>
        static Connection()
        {
            AddExternalPackets(Assembly.GetAssembly(typeof(Packet)));
        }

        /// <summary>
        /// External packets which also should be known by the network lib can be added with this function.
        /// All packets in the network lib are included automatically.
        /// </summary>
        /// <param name="assembly">The assembly to search for included packets.</param>
        internal static void AddExternalPackets(Assembly assembly)
        {
            assembly.GetTypes().ToList().Where(c => c.GetCustomAttributes(typeof(PacketTypeAttribute)).Count() > 0).ToList().
                ForEach(c =>
                {
                    if (!typeByte.ElementAExists(c))
                        typeByte.Add(c, ((PacketTypeAttribute)c.GetCustomAttribute(typeof(PacketTypeAttribute))).Id);
                });
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is alive and able to communicate with the endpoint.
        /// </summary>
        /// <value><c>true</c> if this instance is alive; otherwise, <c>false</c>.</value>
        public bool IsAlive { get { return readStreamThread.IsAlive && writeStreamThread.IsAlive && invokePacketThread.IsAlive; } }

        /// <summary>
        /// Gets or sets if this instance should send in a specific interval a keep alive packet, to ensure
        /// whether there is a connection or not. If set to [false] <see cref="RTT"/> and <see cref="Ping"/> wont be refreshed automatically.
        /// </summary>
        /// <value>Keep alive or not.</value>
        public bool KeepAlive
        {
            get { return keepAlive; }
            set
            {
                keepAlive = value;
                ConfigPing(keepAlive);
            }
        }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int Timeout { get; protected set; } = 5000;

        /// <summary>
        /// Gets the round trip time.
        /// </summary>
        /// <value>The RTT.</value>
        public long RTT { get; private set; } = 0;

        /// <summary>
        /// Gets the ping.
        /// </summary>
        /// <value>The ping.</value>
        public long Ping { get; private set; } = 0;

        /// <summary>
        /// Gets or sets whenever sending a packet to flush the stream immediately.
        /// </summary>
        /// <value>Force to flush or not.</value>
        public bool ForceFlush { get; set; } = true;

        /// <summary>
        /// Registers a packetHandler. This handler will be invoked if this connection
        /// receives the given type.
        /// </summary>
        /// <param name="packetType">Type of the packet we would like to receive.</param>
        /// <param name="handler">The handler which should be invoked.</param>
        public void RegisterPacketHandler(Type packetType, PacketReceivedHandler handler)
        {
            if (!packetHandler.ContainsKey(packetType))
                packetHandler.Add(packetType, handler);
        }

        /// <summary>
        /// UnRegisters a packetHandler. If this connection will receive the given type, it will be ignored,
        /// because there is no handler to invoke anymore.
        /// </summary>
        /// <param name="type">The type.</param>
        public void UnRegisterPacketHandler(Type type)
        {
            if (packetHandler.ContainsKey(type))
                packetHandler.Remove(type);
        }

        /// <summary>
        /// Adds or removes an action which will be invoked if the network dies.
        /// </summary>
        public event Action<CloseReason, Connection> ConnectionClosed
        {
            add { connectionClosed += value; }
            remove { connectionClosed -= value; }
        }

        /// <summary>
        /// Adds or remove an action which will be invoked if the connection
        /// created a new UDP connection. The delivered tcpConnection represents the tcp connection
        /// which was in charge of the new establishment.
        /// </summary>
        public event Action<TcpConnection, UdpConnection> ConnectionEstablished
        {
            add { connectionEstablished += value; }
            remove { connectionEstablished -= value; }
        }

        /// <summary>
        /// Configurations the ping and rtt timers.
        /// </summary>
        private void ConfigPing(bool enable)
        {
#if !DEBUG
            if (enable) nextPingStopWatch.Restart();
            else nextPingStopWatch.Reset();
#endif
        }

        /// <summary>
        /// Sends a ping if there is no ping request already running.
        /// </summary>
        public void SendPing()
        {
            if (currentPingStopWatch.IsRunning) return;
            nextPingStopWatch.Reset();
            currentPingStopWatch.Restart();
            Send(new PingRequest());
        }

        /// <summary>
        /// Converts the given packet into a binary array and sends it to the client's endpoint.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void Send(Packet packet)
        {
            sendPackets.Enqueue(packet);
        }

        /// <summary>
        /// Reads the bytes from the stream.
        /// </summary>
        private void ReadWork()
        {
            try
            {
                while (IsAlive)
                {
                    ushort packetType = BitConverter.ToUInt16(ReadBytes(2), 0);
                    int packetLength = BitConverter.ToInt32(ReadBytes(4), 0);
                    byte[] packetData = ReadBytes(packetLength);
                    Packet receivedPacket = ((Packet)Activator.CreateInstance(typeByte[packetType]));
                    receivedPacket.SetProperties(packetData);
                    receivedPackets.Enqueue(receivedPacket);
                    receivedPacket.Size = packetLength;
                }
            }  catch { }
        }

        /// <summary>
        /// Writes the packets to the stream.
        /// </summary>
        private void WriteWork()
        {
            try
            {
                while (IsAlive)
                {
                    WriteSubWork();
                    Thread.Sleep(CPU_SAVE);
                }
            }
            catch (IOException)
            {
                CloseHandler(CloseReason.Timeout);
            }
            catch { }
        }

        /// <summary>
        /// Writes the packets to the stream.
        /// </summary>
        private void WriteSubWork()
        {
            lock (sendPackets)
            {
                while (sendPackets.Count > 0)
                {
                    Packet packet = null;
                    if (!sendPackets.TryDequeue(out packet))
                        continue;

                    //Prepare some data in the packet.
                    packet.BeforeSend();

                    /*              Packet structure:
                                    1. [16bits] packet type
                                    2. [32bits] packet length
                                    3. [xxbits] packet data                 */

                    byte[] packetData = packet.GetBytes();
                    byte[] packetLength = BitConverter.GetBytes(packetData.Length);
                    byte[] packetByte = new byte[2 + packetLength.Length + packetData.Length];

                    packetByte[0] = (byte)(typeByte[packet.GetType()]);
                    packetByte[1] = (byte)(typeByte[packet.GetType()] >> 8);
                    Array.Copy(packetLength, 0, packetByte, 2, packetLength.Length);
                    Array.Copy(packetData, 0, packetByte, 2 + packetLength.Length, packetData.Length);
                    WriteBytes(packetByte);
                }

                if (KeepAlive && nextPingStopWatch.ElapsedMilliseconds >= PING_INTERVALL)
                {
                    nextPingStopWatch.Reset();
                    currentPingStopWatch.Restart();
                    Send(new PingRequest());
                }
                else if (currentPingStopWatch.ElapsedMilliseconds >= Timeout)
                {
                    ConfigPing(KeepAlive);
                    currentPingStopWatch.Reset();
                    CloseHandler(CloseReason.Timeout);
                }
            }
        }


        /// <summary>
        /// This thread checks for new packets in the queue and delegates them
        /// to the desired delegates, if given.
        /// </summary>
        private void InvokeWork()
        {
            try
            {
                while (IsAlive)
                {
                    while (receivedPackets.Count > 0)
                    {
                        Packet toDelegate = null;
                        if (!receivedPackets.TryDequeue(out toDelegate))
                            continue;

                        toDelegate.BeforeReceive();
                        HandleDefaultPackets(toDelegate);
                    }

                    Thread.Sleep(CPU_SAVE);
                }
            } catch { }
        }

        /// <summary>
        /// Handle the network's packets.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        private void HandleDefaultPackets(Packet packet)
        {
            if (packet.GetType().Equals(typeof(PingRequest)))
            {
                Send(new PingResponse());
                return;
            }
            else if (packet.GetType().Equals(typeof(PingResponse)))
            {
                long elapsedTime = currentPingStopWatch.ElapsedMilliseconds;
                currentPingStopWatch.Reset();
                nextPingStopWatch.Restart();

                Ping = elapsedTime / 2;
                RTT = elapsedTime;
                return;
            }
            else if (packet.GetType().Equals(typeof(CloseRequest)))
            {
                CloseRequest closeRequest = (CloseRequest)packet;
                readStreamThread.Abort();
                writeStreamThread.Abort();
                connectionClosed?.Invoke(closeRequest.CloseReason, this);
                invokePacketThread.Abort();
                CloseSocket();
                return;
            }
            else if (packet.GetType().Equals(typeof(EstablishUdpRequest)))
            {
                EstablishUdpRequest establishUdpRequest = (EstablishUdpRequest)packet;
                IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, GetFreePort());
                Send(new EstablishUdpResponse(udpEndPoint.Port));
                UdpConnection udpConnection = new UdpConnection(new UdpClient(udpEndPoint),
                    new IPEndPoint(IPRemoteEndPoint.Address, establishUdpRequest.UdpPort), true);
                pendingUDPConnections.Enqueue(udpConnection);
                connectionEstablished?.Invoke((TcpConnection)this, udpConnection);
                return;
            }
            else if (packet.GetType().Equals(typeof(EstablishUdpResponseACK)))
            {
                UdpConnection udpConnection = null;
                while (!pendingUDPConnections.TryDequeue(out udpConnection))
                    Thread.Sleep(CPU_SAVE);
                udpConnection.WriteLock = false;
                return;
            }

            if (!packetHandler.ContainsKey(packet.GetType()))
            {
                CloseHandler(CloseReason.UnknownPacket);
                return;
            }

            packetHandler[packet.GetType()].Invoke(packet, this);
        }

        /// <summary>
        /// Closes this connection, but still sends the data on the stream to the bound endpoint.
        /// </summary>
        /// <param name="closeReason">The close reason.</param>
        /// <param name="callCloseEvent">If the instance should call the connectionLost event.</param>
        public void Close(CloseReason closeReason, bool callCloseEvent = false)
        {
            try
            {
                Send(new CloseRequest(closeReason));
                if (callCloseEvent) connectionClosed?.Invoke(closeReason, this);
                WriteSubWork(); //Force to write the remaining packets.
            }
            catch { }
            finally
            {                
                readStreamThread.AbortSave();
                writeStreamThread.AbortSave();
                invokePacketThread.AbortSave();
                CloseSocket();
            }
        }

        /// <summary>
        /// Gets the next free port.
        /// </summary>
        /// <returns>System.Int32.</returns>
        protected int GetFreePort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        /// <summary>
        /// Gets or sets the time to live for the tcp connection.
        /// </summary>
        /// <value>The TTL.</value>
        public abstract short TTL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dual mode]. (Ipv6 + Ipv4)
        /// </summary>
        /// <value><c>true</c> if [dual mode]; otherwise, <c>false</c>.</value>
        public abstract bool DualMode { get; set;}

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Connection"/> is allowed to fragment the frames.
        /// </summary>
        /// <value><c>true</c> if fragment; otherwise, <c>false</c>.</value>
        public abstract bool Fragment { get; set; }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <param name="amount">The amount of bytes we want to read.</param>
        /// <returns>The read bytes.</returns>
        protected abstract byte[] ReadBytes(int amount);

        /// <summary>
        /// Writes bytes to the stream.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        protected abstract void WriteBytes(byte[] bytes);

        /// <summary>
        /// Handles if the connection should be closed, based on the reason.
        /// </summary>
        /// <param name="closeReason">The close reason.</param>
        protected abstract void CloseHandler(CloseReason closeReason);

        /// <summary>
        /// The hop limit. This is compareable to the Ipv4 TTL.
        /// </summary>
        public abstract int HopLimit { get; set; }

        /// <summary>
        /// Gets or sets if the packet should be send with or without any delay.
        /// If disabled, no data will be buffered at all and sent immediately to it's destination.
        /// There is no guarantee that the network performance will be increased.
        /// </summary>
        public abstract bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets if the packet should be sent directly to its destination or not.
        /// </summary>
        public abstract bool IsRoutingEnabled { get; set; }

        /// <summary>
        /// Gets or sets if it should bypass hardware.
        /// </summary>
        public abstract bool UseLoopback { get; set; }

        /// <summary>
        /// Gets the ip address's local endpoint of this connection.
        /// </summary>
        /// <value>The ip end point.</value>
        public abstract IPEndPoint IPLocalEndPoint { get; }

        /// <summary>
        /// Gets the ip address's remote endpoint of this connection.
        /// </summary>
        /// <value>The ip end point.</value>
        public abstract IPEndPoint IPRemoteEndPoint { get; }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        protected abstract void CloseSocket();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}