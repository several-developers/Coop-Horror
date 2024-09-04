using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    [DisallowMultipleComponent]
    public abstract class Entity : NetcodeBehaviour, IEntity
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public MonoBehaviour GetMonoBehaviour() => this;
        public Transform GetTransform() => transform;
        public NetworkObject GetNetworkObject() => NetworkObject;
    }
}