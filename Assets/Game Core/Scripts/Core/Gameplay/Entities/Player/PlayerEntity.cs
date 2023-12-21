using System;
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
        public Vector3 _move;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _timing);
            serializer.SerializeValue(ref _position);
            serializer.SerializeValue(ref _move);
        }
    }
    
    public class PlayerEntity : SNetworkPlayer, IPlayerEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _isDead;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private PlayerState _playerState = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
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

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateOwner()
        {
            if (!IsOwner)
                return;
            
            
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