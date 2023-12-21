using System;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Utilities;
using NetcodePlus;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    [Serializable]
    public struct PlayerState : INetworkSerializable
    {
        public ulong _timing; //Increased by 1 each frame
        public Vector3 _position;
        public Quaternion _rotation;
        public Vector3 _moveDirection;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _timing);
            serializer.SerializeValue(ref _position);
            serializer.SerializeValue(ref _rotation);
            serializer.SerializeValue(ref _moveDirection);
        }
    }
    
    public class PlayerEntity : SNetworkPlayer, IPlayerEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _isDead;

        [Title("Network Settings")]
        [SerializeField, Min(0)]
        private float _syncRefreshRate = 0.05f;

        [SerializeField, Min(0)]
        private float _syncThreshold = 0.1f;

        [SerializeField, Min(0)]
        private float _syncInterpolate = 5f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private PlayerMovement _playerMovement;

        [SerializeField, Required]
        private Transform _cameraPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private PlayerState _playerState = new();
        private SNetworkActions _actions;
        private float _refreshTimer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            var inputSystemListener = GetComponent<InputSystemListener>();
            inputSystemListener.OnMoveEvent += OnMove;
            inputSystemListener.OnLookEvent += OnLook;
            inputSystemListener.OnChangeCursorStateEvent += OnChangeCursorState;
            inputSystemListener.OnChangeCameraLockStateEvent += OnChangeCameraLockState;

            _playerState._position = transform.position;
        }

        private void Start() => ChangeCursorLockState();

        private void Update()
        {
            UpdateOwner();
            UpdateNotOwner();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

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

        public bool IsDead() => _isDead;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            _actions = new SNetworkActions(this);
            _actions.RegisterSerializable("sync", ReceiveSync, NetworkDelivery.Unreliable);
        }

        protected override void OnDespawn()
        {
            base.OnDespawn();
            _actions.Clear();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateOwner()
        {
            if (!IsOwner)
                return;

            Vector3 moveDirection = _playerMovement.GetMoveDirection();
            _playerMovement.Move(moveDirection);

            //Refresh Timer
            _refreshTimer += Time.deltaTime;

            if (_refreshTimer < _syncRefreshRate)
                return;

            //Refresh to other clients
            _refreshTimer = 0f;
            _playerState._timing = _playerState._timing + 1;
            _playerState._position = transform.position;
            _playerState._rotation = transform.rotation;
            _playerState._moveDirection = moveDirection;

            _actions?.Trigger("sync", _playerState); // ReceiveSync(sync_state)
        }

        private void UpdateNotOwner()
        {
            if (IsOwner)
                return;

            Vector3 offset = _playerState._position - transform.position; //Is the character position out of sync?

            if (offset.magnitude > _syncThreshold)
                transform.position = Vector3.MoveTowards(transform.position, _playerState._position, _syncInterpolate * Time.deltaTime);

            if (offset.magnitude > _syncThreshold * 10f)
                transform.position = _playerState._position; //Teleport if too far

            _playerMovement.Move(_playerState._moveDirection);
            transform.rotation = _playerState._rotation;
            //Move(_playerState._move);
        }

        private void ReceiveSync(SerializedData sdata)
        {
            if (IsOwner)
                return;

            PlayerState state = sdata.Get<PlayerState>();

            if (state._timing < _playerState._timing)
                return; //Old timing, ignore package, this means packages arrived in wrong order, prevent glitch

            _playerState = state;
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