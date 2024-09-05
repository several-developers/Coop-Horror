using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    /// <summary>
    /// A class that captures the value of an Addressable's AssetGUID into a string that is serializable/deserializable
    /// by Netcode for GameObjects. This is heavily involved in ClientRpcs/ServerRpcs containing Addressable AssetGUIDs.
    /// </summary>
    public struct AddressableGUID : INetworkSerializable
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        // Could use a byte or a short if client and server has the same list in the same order
        // (use the index in the list instead of the actually 128 bits for a full GUID).
        public FixedString128Bytes Value;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref Value);

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() =>
            Value.ToString();
    }
    
    /// <summary>
    /// A class that implements IEqualityComparer, for AddressableGUID comparisons that don't allocate memory since it
    /// compares integer values instead of strings.
    /// </summary>
    public class AddressableGUIDEqualityComparer : IEqualityComparer<AddressableGUID>
    {
        public int GetHashCode(AddressableGUID addressableGUID) =>
            addressableGUID.GetHashCode();

        public bool Equals(AddressableGUID x, AddressableGUID y) =>
            x.GetHashCode().Equals(y.GetHashCode());
    }
}