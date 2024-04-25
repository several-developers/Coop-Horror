using System;
using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.InputHandlerTEMP;
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
        private PlayerConfigMeta _playerConfig;
        
        [SerializeField, Required]
        private InputReader _inputReader;
        
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private OwnerNetworkAnimator _networkAnimator;

        [SerializeField, Required]
        private NetworkObject _networkObject;

        [SerializeField, Required]
        private Rigidbody _rigidbody;

        [SerializeField, Required]
        private CapsuleCollider _collider;

        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        [SerializeField, Required]
        private Transform _headPoint;

        [SerializeField, Required]
        private Transform _playerItemPivot;

        [SerializeField, Required]
        private Transform _headLookObject;

        [SerializeField, Required, Space(height: 5)]
        private GameObject[] _playerModelParts;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerConfigMeta PlayerConfig => _playerConfig;
        public InputReader InputReader => _inputReader;
        public Animator Animator => _animator;
        public OwnerNetworkAnimator NetworkAnimator => _networkAnimator;
        public NetworkObject NetworkObject => _networkObject;
        public Rigidbody Rigidbody => _rigidbody;
        public CapsuleCollider Collider => _collider;
        public ClientNetworkTransform NetworkTransform => _networkTransform;
        public Transform HeadPoint => _headPoint;
        public Transform PlayerItemPivot => _playerItemPivot;
        public Transform HeadLookObject => _headLookObject;
        public GameObject[] PlayerModelParts => _playerModelParts;
    }
}