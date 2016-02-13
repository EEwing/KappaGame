using System;
using System.Collections.Generic;
using Shared.Networking.Interfaces;

namespace Shared.Networking
{
    public enum PacketTypes : byte
    {
        Ping = 0x0,
        Pong = 0x1
    }

    public class PacketHelper
    {
        private static Dictionary<PacketTypes, Type> _loadedPackets;

        public static Dictionary<PacketTypes, Type> LoadedPackets => _loadedPackets ?? (_loadedPackets = new Dictionary<PacketTypes, Type>());
    
        public static void Add(PacketTypes type, Type packet)
        {
            if (_loadedPackets.ContainsKey(type)) throw new Exception("Packet type already added");
            _loadedPackets.Add(type, packet);
        }

        public static void Remove(PacketTypes type)
        {
            if (!_loadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            _loadedPackets.Remove(type);
        }

        public static Packet Get(PacketTypes type)
        {
            if (!_loadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            return (Packet)Activator.CreateInstance(_loadedPackets[type]);
        }

        public static T Get<T>(PacketTypes type) where T : new()
        {
            if (!_loadedPackets.ContainsKey(type)) throw new Exception("Invalid Packet Type");
            return (T)Activator.CreateInstance(_loadedPackets[type]);
        }
    }
}
