using System;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
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
        private ElevatorTrigger _elevatorTrigger;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private Animator _animator;
        
        [SerializeField, Required]
        private NetworkAnimator _networkAnimator;

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorTrigger ElevatorTrigger => _elevatorTrigger;
        public AnimationObserver AnimationObserver => _animationObserver;
        public Animator Animator => _animator;
        public NetworkAnimator NetworkAnimator => _networkAnimator;
        public ClientNetworkTransform NetworkTransform => _networkTransform;
    }
}