using System;
using System.Net;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Utilities
{
    public static class NetworkUtilities
    {
        // Serialize a INetworkSerializable to bytes
        public static byte[] NetSerialize<T>(T obj, int size = 128) where T : INetworkSerializable, new()
        {
            if (obj == null)
                return Array.Empty<byte>();

            try
            {
                FastBufferWriter writer = new(size, Allocator.Temp, maxSize: 1024 * 1024);
                writer.WriteNetworkSerializable(obj);
                return writer.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError("Serialization error: " + e.Message);
                return Array.Empty<byte>();
            }
        }
        
        // Deserialize a INetworkSerializable from bytes
        public static T NetDeserialize<T>(byte[] bytes) where T : INetworkSerializable, new()
        {
            if (bytes == null || bytes.Length == 0)
                return default;

            try
            {
                FastBufferReader reader = new(bytes, Allocator.Temp);
                reader.ReadNetworkSerializable(out T obj);
                return obj;
            }
            catch (Exception e)
            {
                Debug.LogError("Deserialization error: " + e.Message);
                return default;
            }
        }
        
        public static string DeserializeString(byte[] bytes)
        {
            if (bytes != null)
                return System.Text.Encoding.UTF8.GetString(bytes);
            
            return null;
        }
        
        // Converts a host (either domain or IP) into an IP
        public static string HostToIP(string host)
        {
            bool success = IPAddress.TryParse(host, out IPAddress address);
            
            if (success)
                return address.ToString(); // Already an IP
            
            IPAddress ip = ResolveDns(host); // Not an IP, resolve DNS
            
            if (ip != null)
                return ip.ToString();
            
            return "";
        }
        
        public static IPAddress ResolveDns(string url)
        {
            IPAddress[] ips = Dns.GetHostAddresses(url);
            
            if (ips.Length > 0)
                return ips[0];
            
            return null;
        }
    }
}