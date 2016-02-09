#region Licence - LGPLv3
// ***********************************************************************
// Assembly         : Network
// Author           : Thomas
// Created          : 07-24-2015
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Network.Extensions
{
    /// <summary>
    /// The possible states of a network object.
    /// </summary>
    internal enum ObjectState : byte
    {
        /// <summary>
        /// The object is null.
        /// We didn't write something on the stream.
        /// So we cant read anything from the stream.
        /// </summary>
        NULL = 0x00,
        /// <summary>
        /// The object is not null.
        /// We wrote something on the stream.
        /// So we can read something from the stream.
        /// </summary>
        NOT_NULL = 0xFF
    }

    /// <summary>
    /// Provides extension methods for packets to handle their read and write behaviors.
    /// </summary>
    internal static class PacketExtension
    {
        /// <summary>
        /// Remember a types propertyInfo to save cpu time.
        /// </summary>
        private static Dictionary<Type, PropertyInfo[]> packetProperties = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Converts a given packet to a byte array.
        /// </summary>
        /// <param name="packet">The packet to convert.</param>
        /// <returns>System.Byte[].</returns>
        internal static byte[] GetBytes(this Packet packet)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            GetBytesFromCustomObject(packet, binaryWriter);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Writes the length of the given array onto the given stream.
        /// Reads the array out of the property and calls the <see cref="GetBytesFromCustomObject"/> to write the object data.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryWriter">The binary writer.</param>
        /// <returns>System.Byte[].</returns>
        private static void GetBytesFromArray(object obj, PropertyInfo propertyInfo, BinaryWriter binaryWriter)
        {
            Type arrayType = propertyInfo.PropertyType.GetElementType();
            Array propertyData = (Array)propertyInfo.GetValue(obj);
            binaryWriter.Write(propertyData?.Length ?? 0);

            if (arrayType.IsClass) propertyData.GetEnumerator().ToList<object>().ForEach(p => GetBytesFromCustomObject(p, binaryWriter));
            else propertyData.GetEnumerator().ToList<object>().ForEach(p => { dynamic targetType = p; binaryWriter.Write(targetType); });
        }

        /// <summary>
        /// Writes the length of the given list onto the given stream.
        /// Reads the list out of the property and calls the <see cref="GetBytesFromCustomObject"/> to write the object data.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryWriter">The binary writer.</param>
        /// <returns>System.Byte[].</returns>
        private static void GetBytesFromList(object obj, PropertyInfo propertyInfo, BinaryWriter binaryWriter)
        {
            Type listType = propertyInfo.PropertyType.GetGenericArguments()[0];
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
            ((IEnumerable)propertyInfo.GetValue(obj))?.GetEnumerator().ToList<object>().ForEach(o => list.Add(o));
            binaryWriter.Write(list.Count);

            if (listType.IsClass) list.GetEnumerator().ToList<object>().ForEach(o => GetBytesFromCustomObject(o, binaryWriter));
            else list.GetEnumerator().ToList<object>().ForEach(o => { dynamic targetType = o; binaryWriter.Write(targetType); });
        }

        /// <summary>
        /// Gets the bytes from custom object. It searches for all propertyInfos and calls the next method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="binaryWriter">The binary writer.</param>
        private static void GetBytesFromCustomObject(object obj, BinaryWriter binaryWriter)
        {
            GetPacketProperties(obj).ToList().ForEach(p => GetBytesFromCustomObject(obj, p, binaryWriter));
        }

        /// <summary>
        /// Writes the data of all the properties in place on the given binary stream.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryWriter">The binary writer.</param>
        /// <returns>System.Byte[].</returns>
        private static void GetBytesFromCustomObject(object obj, PropertyInfo propertyInfo, BinaryWriter binaryWriter)
        {
            dynamic propertyValue = propertyInfo.GetValue(obj);
            if (propertyInfo.PropertyType.IsEnum) //Enums are an exception.
                propertyValue = (int)propertyValue;
            if (propertyInfo.PropertyType.IsArray)
                GetBytesFromArray(obj, propertyInfo, binaryWriter);
            else if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(List<>)))
                GetBytesFromList(obj, propertyInfo, binaryWriter);
            else if (!IsPrimitive(propertyInfo) && propertyValue != null) //Primitive or object
            { 
                binaryWriter.Write((byte)ObjectState.NOT_NULL); //Mark it as a NOT NULL object.
                GetBytesFromCustomObject(propertyValue, binaryWriter);
            }
            else if (!IsPrimitive(propertyInfo) && propertyValue == null)
                binaryWriter.Write((byte)ObjectState.NULL); //Mark it as a NULL object.
            else binaryWriter.Write(propertyValue);
        }

        /// <summary>
        /// Applies the byte array to the packets properties.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="data">The data which should be applied.</param>
        internal static void SetProperties(this Packet packet, byte[] data)
        {
            MemoryStream memoryStream = new MemoryStream(data, 0, data.Length);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            ReadObjectFromStream(packet, binaryReader);
        }

        /// <summary>
        /// Reads the length of the array from the stream and creates the array instance.
        /// Also fills the array by using recursion.  
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Array.</returns>
        private static Array ReadArrayFromStream(object obj, PropertyInfo propertyInfo, BinaryReader binaryReader)
        {
            int arraySize = binaryReader.ReadInt32();
            Type arrayType = propertyInfo.PropertyType.GetElementType();
            Array propertyData = Array.CreateInstance(arrayType, arraySize);
            for (int i = 0; i < arraySize; i++)
            {
                if (arrayType.IsClass) propertyData.SetValue(ReadObjectFromStream(Activator.CreateInstance(arrayType), binaryReader), i);
                else propertyData.SetValue(ReadPrimitiveFromStream(arrayType, binaryReader), i);
            }

            return propertyData;
        }

        /// <summary>
        /// Reads the length of the list from the stream and creates the list instance.
        /// Also fills the list by using recursion.  
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Collections.IList.</returns>
        private static IList ReadListFromStream(object obj, PropertyInfo propertyInfo, BinaryReader binaryReader)
        {
            int listSize = binaryReader.ReadInt32();
            Type listType = propertyInfo.PropertyType.GetGenericArguments()[0];
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
            for (int i = 0; i < listSize; i++)
            {
                if (listType.IsClass) list.Add(ReadObjectFromStream(Activator.CreateInstance(listType), binaryReader));
                else list.Add(ReadPrimitiveFromStream(listType, binaryReader));
            }

            return list;
        }

        /// <summary>
        /// Reads all the properties from an object and calls the <see cref="ReadObjectFromStream"/> method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Object.</returns>
        private static object ReadObjectFromStream(object obj, BinaryReader binaryReader)
        {
            GetPacketProperties(obj).ToList().ForEach(p => p.SetValue(obj, ReadObjectFromStream(obj, p, binaryReader)));
            return obj; //All properties set with in place.
        }

        /// <summary>
        /// Reads the object from stream. Can differ between:
        /// - Primitives
        /// - Lists
        /// - Arrays
        /// - Classes (None list + arrays)
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Object.</returns>
        private static object ReadObjectFromStream(object obj, PropertyInfo propertyInfo, BinaryReader binaryReader)
        {
            if (propertyInfo.PropertyType.IsArray)
                return ReadArrayFromStream(obj, propertyInfo, binaryReader);
            else if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition().Equals(typeof(List<>)))
                return ReadListFromStream(obj, propertyInfo, binaryReader);
            else if (!IsPrimitive(propertyInfo))
            {
                ObjectState objectState = (ObjectState)binaryReader.ReadByte();
                if(objectState == ObjectState.NOT_NULL)
                    return ReadObjectFromStream(Activator.CreateInstance(propertyInfo.PropertyType), binaryReader);
                return null; //The object we received is null. So return nothing.
            }
            else return ReadPrimitiveFromStream(propertyInfo, binaryReader);
        }

        /// <summary>
        /// Reads a primitive from a stream.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Object.</returns>
        private static object ReadPrimitiveFromStream(PropertyInfo propertyInfo, BinaryReader binaryReader)
        {
            return ReadPrimitiveFromStream(propertyInfo.PropertyType, binaryReader);
        }

        /// <summary>
        /// Reads a primitive from a stream.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="binaryReader">The binary reader.</param>
        /// <returns>System.Object.</returns>
        private static object ReadPrimitiveFromStream(Type type, BinaryReader binaryReader)
        {
            if (type.Equals(typeof(String)))
                return binaryReader.ReadString();
            else if (type.Equals(typeof(Int16)))
                return binaryReader.ReadInt16();
            else if (type.Equals(typeof(Int32)))
                return binaryReader.ReadInt32();
            else if (type.Equals(typeof(Int64)))
                return binaryReader.ReadInt64();
            else if (type.Equals(typeof(Boolean)))
                return binaryReader.ReadBoolean();
            else if (type.Equals(typeof(Byte)))
                return binaryReader.ReadByte();
            else if (type.Equals(typeof(Char)))
                return binaryReader.ReadChar();
            else if (type.Equals(typeof(Decimal)))
                return binaryReader.ReadDecimal();
            else if (type.Equals(typeof(Double)))
                return binaryReader.ReadDouble();
            else if (type.Equals(typeof(Single)))
                return binaryReader.ReadSingle();
            else if (type.Equals(typeof(UInt16)))
                return binaryReader.ReadUInt16();
            else if (type.Equals(typeof(UInt32)))
                return binaryReader.ReadUInt32();
            else if (type.Equals(typeof(UInt64)))
                return binaryReader.ReadUInt64();
            else if (type.IsEnum)
                return binaryReader.ReadInt32();

            //Only primitive types are supported in this method.
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the specified property information is primitive.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>System.Boolean.</returns>
        private static bool IsPrimitive(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.Equals(typeof(String)) || propertyInfo.PropertyType.Equals(typeof(Int16)) ||
                propertyInfo.PropertyType.Equals(typeof(Int32)) || propertyInfo.PropertyType.Equals(typeof(Int64)) ||
                propertyInfo.PropertyType.Equals(typeof(Boolean)) || propertyInfo.PropertyType.Equals(typeof(Byte)) ||
                propertyInfo.PropertyType.Equals(typeof(Char)) || propertyInfo.PropertyType.Equals(typeof(Decimal)) ||
                propertyInfo.PropertyType.Equals(typeof(Double)) || propertyInfo.PropertyType.Equals(typeof(Single)) ||
                propertyInfo.PropertyType.Equals(typeof(UInt16)) || propertyInfo.PropertyType.Equals(typeof(UInt32)) ||
                propertyInfo.PropertyType.Equals(typeof(UInt64)) || propertyInfo.PropertyType.IsEnum)
                return true;
            return false;
        }

        /// <summary>
        /// Extracts all properties from a packet which do not have a packetIgnoreProperty attribute.
        /// </summary>
        /// <param name="packet">The packet to extract the property infos</param>
        /// <returns>The property infos.</returns>
        private static PropertyInfo[] GetPacketProperties(object packet)
        {
            lock(packetProperties)
            {
                if (packetProperties.ContainsKey(packet.GetType()))
                    return packetProperties[packet.GetType()];
                PropertyInfo[] propInfos = packet.GetType().GetProperties().ToList().Where(p => p.GetCustomAttribute(typeof(PacketIgnorePropertyAttribute)) == null).ToArray();
                packetProperties.Add(packet.GetType(), propInfos);
                return GetPacketProperties(packet);
            }
        }
    }
}
