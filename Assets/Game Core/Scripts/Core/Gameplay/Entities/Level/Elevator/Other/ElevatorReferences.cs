using System;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode.Components;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    [Serializable]
    public class ElevatorReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Animator _animator;
        
        [SerializeField, Required]
        private NetworkAnimator _networkAnimator;

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Animator Animator => _animator;
        public NetworkAnimator NetworkAnimator => _networkAnimator;
        public ClientNetworkTransform NetworkTransform => _networkTransform;
    }
}