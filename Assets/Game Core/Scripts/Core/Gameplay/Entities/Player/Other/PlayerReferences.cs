using System;
using ECM2;
using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.EntitiesSystems.Health;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    [Serializable]
    public class PlayerReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        [SerializeField, Required]
        private OwnerNetworkAnimator _networkAnimator;

        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private Rigidbody _rigidbody;

        [SerializeField, Required]
        private CapsuleCollider _collider;

        [SerializeField, Required]
        private Transform _headLookAtObject;

        [SerializeField, Required]
        private Transform _leftHandItemsHolder;

        [SerializeField, Required]
        private Transform _rightHandItemsHolder;

        [SerializeField, Required]
        private Character _character;

        [SerializeField, Required]
        private PlayerMovementController _playerMovementController;

        [SerializeField, Required]
        private MyAnimationController _animationController;

        [SerializeField, Required]
        private HealthSystem _healthSystem;

        [SerializeField, Required, Space(height: 5)]
        private SkinnedMeshRenderer[] _hiddenMeshes;
        
        [SerializeField, Required]
        private GameObject[] _localPlayerActiveObjects;
        
        [SerializeField, Required]
        private GameObject[] _localPlayerInactiveObjects;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ClientNetworkTransform NetworkTransform => _networkTransform;
        public OwnerNetworkAnimator NetworkAnimator => _networkAnimator;
        public Animator Animator => _animator;
        public Rigidbody Rigidbody => _rigidbody;
        public CapsuleCollider Collider => _collider;
        public Transform HeadLookAtObject => _headLookAtObject;
        public Transform LeftHandItemsHolder => _leftHandItemsHolder;
        public Transform RightHandItemsHolder => _rightHandItemsHolder;
        public Character Character => _character;
        public PlayerMovementController PlayerMovementController => _playerMovementController;
        public MyAnimationController AnimationController => _animationController;
        public HealthSystem HealthSystem => _healthSystem;
        public SkinnedMeshRenderer[] HiddenMeshes => _hiddenMeshes;
        public GameObject[] LocalPlayerActiveObjects => _localPlayerActiveObjects;
        public GameObject[] LocalPlayerInactiveObjects => _localPlayerInactiveObjects;
    }
}