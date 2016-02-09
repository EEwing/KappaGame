﻿#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : Network
// Author           : Thomas
// Created          : 07-24-2015
//
// Last Modified By : Thomas
// Last Modified On : 07-26-2015
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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network.Enums;
using Network.Extensions;

namespace Network
{
    /// <summary>
    /// This class contains a udp connection to the given udp client.
    /// It provides convenient methods to send and receive objects with a minimal serialization header.
    /// </summary>
    public class UdpConnection : Connection
    {
        private Socket socket;
        private UdpClient client;
        private IPEndPoint ipEndPoint;

        /// <summary>
        /// The received data cache.
        /// </summary>
        private List<byte> receivedBytes = new List<byte>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpConnection"/> class.
        /// </summary>
        /// <param name="udpClient">The UDP client.</param>
        /// <param name="endPoint">The endPoint where we want to receive the data.</param>
        internal UdpConnection(UdpClient udpClient, IPEndPoint remoteEndPoint, bool writeLock = false)
            : base()
        {
            client = udpClient;
            WriteLock = writeLock;
            socket = client.Client;
            ipEndPoint = remoteEndPoint;
            client.Connect(remoteEndPoint);

            KeepAlive = false;
            socket.SendTimeout = 0;
            socket.ReceiveTimeout = 0;
            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

            Init();
        }

        /// <summary>
        /// Gets or sets the time to live for the tcp connection.
        /// </summary>
        /// <value>The TTL.</value>
        public override short TTL { get { return client.Ttl; } set { client.Ttl = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether [dual mode]. (Ipv6 + Ipv4)
        /// </summary>
        /// <value><c>true</c> if [dual mode]; otherwise, <c>false</c>.</value>
        public override bool DualMode { get { return socket.DualMode; } set { socket.DualMode = value; } }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UdpConnection"/> is allowed to fragment the frames.
        /// </summary>
        /// <value><c>true</c> if fragment; otherwise, <c>false</c>.</value>
        public override bool Fragment { get { return !socket.DontFragment; } set { socket.DontFragment = !value; } }

        /// <summary>
        /// Gets or sets if a UDP packet checksum should be created.
        /// </summary>
        public bool IsChecksumEnabled
        {
            get { return (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoChecksum) == 0; }
            set { socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoChecksum, value ? 0 : -1); }
        }

        /// <summary>
        /// The hop limit. This is compareable to the Ipv4 TTL.
        /// </summary>
        public override int HopLimit
        {
            get { return (int)socket.GetSocketOption(SocketOptionLevel.Udp, SocketOptionName.HopLimit); }
            set { socket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.HopLimit, value); }
        }

        /// <summary>
        /// Gets or sets if the packet should be send with or without any delay.
        /// If disabled, no data will be buffered at all and sent immediately to it's destination.
        /// There is no guarantee that the network performance will be increased.
        /// </summary>
        public override bool NoDelay
        {
            get { return client.Client.NoDelay; }
            set { client.Client.NoDelay = value; }
        }

        /// <summary>
        /// Gets or sets if it should bypass hardware.
        /// </summary>
        public override bool UseLoopback
        {
            get { return (bool)socket.GetSocketOption(SocketOptionLevel.Udp, SocketOptionName.UseLoopback); }
            set { socket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.UseLoopback, value); }
        }

        /// <summary>
        /// Gets or sets if the packet should be sent directly to its destination or not.
        /// </summary>
        public override bool IsRoutingEnabled
        {
            get { return !(bool)socket.GetSocketOption(SocketOptionLevel.Udp, SocketOptionName.DontRoute); }
            set { socket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.DontRoute, !value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [write lock].
        /// </summary>
        /// <value><c>true</c> if [write lock]; otherwise, <c>false</c>.</value>
        internal bool WriteLock { get; set; }

        /// <summary>
        /// Reads bytes from the stream.
        /// </summary>
        /// <param name="amount">The amount of bytes we want to read.</param>
        /// <returns>The read bytes.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override byte[] ReadBytes(int amount)
        {
            while (receivedBytes.Count < amount)
            {
                receivedBytes.AddRange(client.Receive(ref ipEndPoint).GetEnumerator().ToList<byte>());
                Thread.Sleep(CPU_SAVE);
            }

            byte[] data = new byte[amount];
            receivedBytes.CopyTo(0, data, 0, data.Length);
            receivedBytes.RemoveRange(0, data.Length);
            return data;
        }

        /// <summary>
        /// Writes bytes to the stream.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void WriteBytes(byte[] bytes)
        {
            while (WriteLock)
                Thread.Sleep(CPU_SAVE * 2);
            client.Send(bytes, bytes.Length);
        }

        /// <summary>
        /// Handles if the connection should be closed, based on the reason.
        /// </summary>
        /// <param name="closeReason">The close reason.</param>
        protected override void CloseHandler(CloseReason closeReason)
        {
            Close(closeReason, true);
        }

        /// <summary>
        /// Gets the ip address's local endpoint of this connection.
        /// </summary>
        /// <value>The ip end point.</value>
        public override IPEndPoint IPLocalEndPoint { get { return (IPEndPoint)client.Client.LocalEndPoint; } }

        /// <summary>
        /// Gets the ip address's remote endpoint of this connection.
        /// </summary>
        /// <value>The ip end point.</value>
        public override IPEndPoint IPRemoteEndPoint { get { return (IPEndPoint)client.Client.RemoteEndPoint; } }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void CloseSocket()
        {
            socket.Close();
            client.Close();
        }
    }
}
