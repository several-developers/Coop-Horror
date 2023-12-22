using System;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
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

        private void Awake()
        {
            var inputSystemListener = GetComponent<InputSystemListener>();
            inputSystemListener.OnMoveEvent += OnMove;
            inputSystemListener.OnLookEvent += OnLook;
            inputSystemListener.OnChangeCursorStateEvent += OnChangeCursorState;
            inputSystemListener.OnChangeCameraLockStateEvent += OnChangeCameraLockState;
        }

        private void Start() => ChangeCursorLockState();

        private void Update()
        {
            UpdateOwner();
            UpdateNotOwner();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                PlayerCamera.Instance.SetTarget(transform);
            
            base.OnNetworkSpawn();
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

        private static void ChangeCursorLockState() =>
            GameUtilities.ChangeCursorLockState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            OnMovementVectorChangedEvent?.Invoke(movementVector);

        private void OnLook(Vector2 lookVector)
        {
        }

        private void OnChangeCameraLockState()
        {
        }

        private static void OnChangeCursorState() => ChangeCursorLockState();
    }
}