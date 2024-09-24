using System;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    [Serializable]
    public class ElevatorReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Animator Animator => _animator;
        public ClientNetworkTransform NetworkTransform => _networkTransform;
    }
}