using System;
using System.Collections.Generic;
using Shared.Networking.Interfaces;

namespace Shared.Networking
{
    public enum PacketTypes : byte
    {
        // 0-F = Utility packets
        Ping = 0x0,
        Pong = 0x1,

        // 10-1F = Player packets
        ConnectionAccept = 0x10,
        EntityState = 0x11,

        // 20-2F = Map packets
        MapState = 0x20
    }

    public class PacketHelper
    {
        private static Dictionary<PacketTypes, Type> _loadedPackets;

        public static Dictionary<PacketTypes, Type> LoadedPackets => _loadedPackets ?? (_loadedPackets = new Dictionary<PacketTypes, Type>());
    
        public static void Add(PacketTypes type, Type packet)
        {
            if (LoadedPackets.ContainsKey(type)) throw new Exception("Packet type already added");
            _loadedPackets.Add(type, packet);
        }

        public static void Remove(PacketTypes type)
        {
            if (!LoadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            LoadedPackets.Remove(type);
        }

        public static Packet Get(PacketTypes type)
        {
            if (!LoadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            return (Packet)Activator.CreateInstance(LoadedPackets[type]);
        }

        public static T Get<T>(PacketTypes type) where T : new()
        {
            if (!LoadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            return (T)Activator.CreateInstance(LoadedPackets[type]);
        }
    }
}
