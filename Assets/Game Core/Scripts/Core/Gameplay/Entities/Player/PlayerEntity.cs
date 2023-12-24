using System;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Gameplay.Managers;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerEntity : NetworkBehaviour, IPlayerEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _isDead;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private NetworkObject _networkObject;
        
        [SerializeField, Required]
        private PlayerMovement _playerMovement;

        [SerializeField, Required]
        private Transform _cameraPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Update()
        {
            UpdateOwner();
            UpdateNotOwner();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            Init();
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            DespawnOwner();
            base.OnNetworkDespawn();
        }

        public void Setup()
        {
            float reloadTime = 1.5f;
            float reloadAnimationTime = _animator.GetAnimationTime(AnimatorHashes.ReloadingAnimation);
            float reloadTimeMultiplier = reloadAnimationTime / reloadTime; // 1, 0.5, 1 / 0.5

            //_animator.SetFloat(id: AnimatorHashes.ReloadMultiplier, reloadTimeMultiplier);
        }

        public void TakeDamage(IEntity source, float damage)
        {
            //_animator.SetTrigger(id: AnimatorHashes.HitReaction);
        }

        public Transform GetTransform() => transform;
        
        public Animator GetAnimator() => _animator;

        public NetworkObject GetNetworkObject() => _networkObject;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            InitOwner();
        }

        private void InitOwner()
        {
            if (!IsOwner)
                return;

            var playerInput = GetComponent<PlayerInput>();
            
            InputSystemManager.Init(playerInput);
            PlayerCamera.Instance.SetTarget(transform);
            
            InputSystemManager.OnMoveEvent += OnMove;
        }

        private void UpdateOwner()
        {
            if (!IsOwner)
                return;

            _playerMovement.HandleMovement();
        }

        private void UpdateNotOwner()
        {
            if (IsOwner)
                return;
        }

        private void DespawnOwner()
        {
            if (!IsOwner)
                return;
            
            PlayerCamera.Instance.RemoveTarget();
            InputSystemManager.OnMoveEvent -= OnMove;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            OnMovementVectorChangedEvent?.Invoke(movementVector);
    }
}