#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : Network
// Author           : Thomas
// Created          : 07-26-2015
//
// Last Modified By : Thomas
// Last Modified On : 08-09-2015
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
using System;
using System.Linq;
using System.Collections.Generic;
using Network.Packets;
using Network.Enums;
using System.Timers;
using System.Threading.Tasks;

namespace Network
{
    /// <summary>
    /// The connection container contains a tcp and x udp connections.
    /// It provides convenient methods to reduce the number of code lines which are needed to manage all the connections.
    /// By default one tcp and one udp connection will be created automatically.
    /// </summary>
    public class ClientConnectionContainer : ConnectionContainer
    {
        /// <summary>
        /// The reconnect timer. Invoked if we lose the connection.
        /// </summary>
        private Timer reconnectTimer;

        /// <summary>
        /// The connections we have to deal with.
        /// </summary>
        private TcpConnection tcpConnection;
        private UdpConnection udpConnection;

        /// <summary>
        /// If there is no connection yet, save the packets in this buffer.
        /// </summary>
        private List<Packet> sendSlowBuffer = new List<Packet>();
        private List<Packet> sendFastBuffer = new List<Packet>();

        /// <summary>
        /// Cache all the handlers to apply them after we got a new connection.
        /// </summary>
        private List<Tuple<Type, PacketReceivedHandler>> tcpPacketHandlerBuffer = new List<Tuple<Type, PacketReceivedHandler>>();
        private List<Tuple<Type, PacketReceivedHandler>> udpPacketHandlerBuffer = new List<Tuple<Type, PacketReceivedHandler>>();

        /// <summary>
        /// Occurs when we get or lose a tcp or udp connection.
        /// </summary>
        private event Action<Connection, ConnectionType, CloseReason> connectionLost;
        private event Action<Connection, ConnectionType> connectionEstablished;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionContainer"/> class.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        internal ClientConnectionContainer(string ipAddress, int port)
            : base(ipAddress, port)
        {
            TryConnect();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientConnectionContainer"/> class.
        /// </summary>
        /// <param name="tcpConnection">The TCP connection.</param>
        /// <param name="udpConnection">The UDP connection.</param>
        internal ClientConnectionContainer(TcpConnection tcpConnection, UdpConnection udpConnection)
            : base(tcpConnection.IPRemoteEndPoint.Address.ToString(), tcpConnection.IPRemoteEndPoint.Port)
        {
            this.tcpConnection = tcpConnection;
            this.udpConnection = udpConnection;
            TryConnect();
        }

        /// <summary>
        /// Gets or sets if this container should automatically reconnect to the endpoint if the connection has been closed.
        /// </summary>
        /// <value><c>true</c> if [automatic reconnect]; otherwise, <c>false</c>.</value>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Gets or sets the reconnect interval in [ms].
        /// </summary>
        /// <value>The reconnect interval.</value>
        public int ReconnectInterval { get; set; } = 2500;

        /// <summary>
        /// Gets the TCP connection.
        /// </summary>
        /// <value>The TCP connection.</value>
        public TcpConnection TcpConnection { get { return tcpConnection; } }

        /// <summary>
        /// Gets the UDP connections.
        /// </summary>
        /// <value>The UDP connections.</value>
        public UdpConnection UdpConnection { get { return udpConnection; } }

        /// <summary>
        /// Will be called if a TCP or an UDP connection has been successfully established.
        /// </summary>
        public event Action<Connection, ConnectionType> ConnectionEstablished
        {
            add { connectionEstablished += value; }
            remove { connectionEstablished -= value; }
        }

        /// <summary>
        /// Will be called if a TCP or an UDP connection has been lost.
        /// </summary>
        public event Action<Connection, ConnectionType, CloseReason> ConnectionLost
        {
            add { connectionLost += value; }
            remove { connectionLost -= value; }
        }

        /// <summary>
        /// Gets if the tcp connection is alive.
        /// </summary>
        /// <value>The is alive_ TCP.</value>
        public bool IsAlive_TCP
        {
            get
            {
                if (tcpConnection == null)
                    return false;
                return tcpConnection.IsAlive;
            }
        }

        /// <summary>
        /// Gets if the udp connection is alive.
        /// </summary>
        /// <value>The is alive_ UDP.</value>
        public bool IsAlive_UDP
        {
            get
            {
                if (udpConnection == null)
                    return false;
                return udpConnection.IsAlive;
            }
        }

        /// <summary>
        /// Gets if the tcp and udp connection is alive.
        /// </summary>
        /// <value>The is alive.</value>
        public bool IsAlive { get { return IsAlive_TCP && IsAlive_UDP; } }

        /// <summary>
        /// Tries to connect to the given endpoint.
        /// </summary>
        /// <param name="e">e.</param>
        /// <param name="sender">sender.</param>
        private void TryToConnect(object sender, ElapsedEventArgs e)
        {
            TryConnect();
        }

        /// <summary>
        /// Tries to connect to the given endpoint.
        /// </summary>
        private async void TryConnect()
        {
            if (reconnectTimer != null) reconnectTimer.Stop();

            if (tcpConnection == null || !tcpConnection.IsAlive)
                await OpenNewTCPConnection();
            if ((udpConnection == null || !udpConnection.IsAlive) && IsAlive_TCP)
                await OpenNewUDPConnection();
        }

        /// <summary>
        /// Registers a tcp packetHandler. This handler will be invoked if this connection
        /// receives the given type.
        /// </summary>
        /// <param name="packetType">Type of the packet we would like to receive.</param>
        /// <param name="handler">The handler which should be invoked.</param>
        public void TCP_RegisterPacketHandler(Type packetType, PacketReceivedHandler handler)
        {
            if(tcpConnection != null && tcpConnection.IsAlive)
                tcpConnection.RegisterPacketHandler(packetType, handler);
            tcpPacketHandlerBuffer.Add(new Tuple<Type, PacketReceivedHandler>(packetType, handler));
        }

        /// <summary>
        /// Registers a udp packetHandler. This handler will be invoked if this connection
        /// receives the given type.
        /// </summary>
        /// <param name="packetType">Type of the packet we would like to receive.</param>
        /// <param name="handler">The handler which should be invoked.</param>
        public void UDP_RegisterPacketHandler(Type packetType, PacketReceivedHandler handler)
        {
            if (udpConnection != null && udpConnection.IsAlive)
                udpConnection.RegisterPacketHandler(packetType, handler);
            udpPacketHandlerBuffer.Add(new Tuple<Type, PacketReceivedHandler>(packetType, handler));
        }

        /// <summary>
        /// Registers a udp and a tcp packetHandler. This handler will be invoked if this connection
        /// receives the given type.
        /// </summary>
        /// <param name="packetType">Type of the packet we would like to receive.</param>
        /// <param name="handler">The handler which should be invoked.</param>
        public void RegisterPacketHandler(Type packetType, PacketReceivedHandler handler)
        {
            TCP_RegisterPacketHandler(packetType, handler);
            UDP_RegisterPacketHandler(packetType, handler);
        }

        /// <summary>
        /// UnRegisters a tcp packetHandler. If this connection will receive the given type, it will be ignored,
        /// because there is no handler to invoke anymore.
        /// </summary>
        /// <param name="type">The type.</param>
        public void TCP_UnRegisterPacketHandler(Type packetType)
        {
            tcpConnection.UnRegisterPacketHandler(packetType);
            tcpPacketHandlerBuffer.Remove(tcpPacketHandlerBuffer.Where(t => t.Item1.Equals(packetType)).ToArray()[0]);
        }

        /// <summary>
        /// UnRegisters a tcp packetHandler. If this connection will receive the given type, it will be ignored,
        /// because there is no handler to invoke anymore.
        /// </summary>
        /// <param name="type">The type.</param>
        public void UDP_UnRegisterPacketHandler(Type packetType)
        {
            udpConnection.UnRegisterPacketHandler(packetType);
            udpPacketHandlerBuffer.Remove(udpPacketHandlerBuffer.Where(t => t.Item1.Equals(packetType)).ToArray()[0]);
        }

        /// <summary>
        /// UnRegisters a udp and a tcp packetHandler. If this connection will receive the given type, it will be ignored,
        /// because there is no handler to invoke anymore.
        /// </summary>
        /// <param name="type">The type.</param>
        public void UnRegisterPacketHandler(Type packetType)
        {
            TCP_UnRegisterPacketHandler(packetType);
            UDP_UnRegisterPacketHandler(packetType);
        }

        /// <summary>
        /// Closes all connections which are bound to this object.
        /// </summary>
        /// <param name="closeReason">The close reason.</param>
        /// <param name="callCloseEvent">If the instance should call the connectionLost event.</param>
        public void Shutdown(CloseReason closeReason, bool callCloseEvent = false)
        {
            tcpConnection.Close(closeReason, callCloseEvent);
            udpConnection.Close(closeReason, callCloseEvent);
        }

        /// <summary>
        /// Opens the new TCP connection and applies the already registered packet handlers.
        /// </summary>
        private async Task<bool> OpenNewTCPConnection()
        {
            Tuple<TcpConnection, ConnectionResult> result = await ConnectionFactory.CreateTcpConnectionAsync(IPAddress, Port);
            if (result.Item2 != ConnectionResult.Connected) { Reconnect(); return false; }
            tcpConnection = result.Item1;
            tcpPacketHandlerBuffer.ForEach(t => tcpConnection.RegisterPacketHandler(t.Item1, t.Item2));
            tcpConnection.ConnectionClosed += (c, cc) => { Reconnect(); connectionLost?.Invoke(tcpConnection, ConnectionType.TCP, c); };
            sendSlowBuffer.ForEach(tcpConnection.Send);
            connectionEstablished?.Invoke(tcpConnection, ConnectionType.TCP);
            return true;
        }

        /// <summary>
        /// Opens the new UDP connection and applies the already registered packet handlers.
        /// </summary>
        private async Task<bool> OpenNewUDPConnection()
        {
            Tuple<UdpConnection, ConnectionResult> result = await ConnectionFactory.CreateUdpConnectionAsync(tcpConnection);
            if (result.Item2 != ConnectionResult.Connected) { Reconnect(); return false; }
            udpConnection = result.Item1;
            udpPacketHandlerBuffer.ForEach(u => udpConnection.RegisterPacketHandler(u.Item1, u.Item2));
            udpConnection.ConnectionClosed += (c, cc) => { Reconnect(); connectionLost?.Invoke(udpConnection, ConnectionType.UDP, c); };
            sendFastBuffer.ForEach(s => udpConnection.Send(s));
            connectionEstablished?.Invoke(udpConnection, ConnectionType.UDP);
            return true;
        }

        /// <summary>
        /// Sends a ping over the tcp connection.
        /// </summary>
        public void SendPing()
        {
            if (tcpConnection != null && !tcpConnection.IsAlive)
                sendSlowBuffer.Add(new PingRequest());
            else tcpConnection.SendPing();
        }

        /// <summary>
        /// Sends a packet via. TCP or UDP depending on the type.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="type">The transmission type.</param>
        public void Send(Packet packet, ConnectionType type)
        {
            if (type == ConnectionType.TCP)
                SendSlow(packet);
            else if (type == ConnectionType.UDP)
                SendFast(packet);
            else throw new ArgumentException("The given enum doesn't exist");
        }

        /// <summary>
        /// Sends a packet via. TCP by default.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void Send(Packet packet)
        {
            SendSlow(packet);
        }

        /// <summary>
        /// Sends the given packet over the tcp connection.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void SendSlow(Packet packet)
        {
            if (tcpConnection == null || !tcpConnection.IsAlive)
                sendSlowBuffer.Add(packet);
            else tcpConnection.Send(packet);
        }

        /// <summary>
        /// Sends the given packet over the udp connection.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public void SendFast(Packet packet)
        {
            if (udpConnection == null || !udpConnection.IsAlive)
                sendFastBuffer.Add(packet);
            else udpConnection.Send(packet);
        }

        /// <summary>
        /// Reconnects the tcp and or the udp connection.
        /// </summary>
        /// <param name="forceReconnect">If AutoReconnect is disabled, force a reconnect by settings forceReconnect to true.</param>
        public void Reconnect(bool forceReconnect = false)
        {
            if (IsAlive) return;
            reconnectTimer = new Timer();
            reconnectTimer.Interval = ReconnectInterval;
            reconnectTimer.Elapsed += TryToConnect;
            reconnectTimer.Start();

            if (forceReconnect || AutoReconnect)
                reconnectTimer.Start();
        }

        public override string ToString()
        {
            return $"ClientConnectionContainer. TCP is alive {IsAlive_TCP}. UDP is alive {IsAlive_UDP}. Server IPAddress {IPAddress} Port {Port.ToString()}";
        }
    }
}
