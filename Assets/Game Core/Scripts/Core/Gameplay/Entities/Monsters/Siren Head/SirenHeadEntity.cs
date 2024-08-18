using System;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.SirenHead.States;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Systems.Footsteps;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.SirenHead
{
    public class SirenHeadEntity : MonsterEntityBase
    {
        public enum SFXType
        {
            // _ = 0,
            Footsteps = 1,
            Roar = 2
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle, IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _timeCycle = timeCycle;
            _sirenHeadAIConfig = monstersAIConfigsProvider.GetSirenHeadAIConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [BoxGroup(ReferencesTitle, showLabel: false), SerializeField]
        private References _references;

        // FIELDS: --------------------------------------------------------------------------------

        private const string ReferencesTitle = "References";
        
        private ITimeCycle _timeCycle;
        private SirenHeadAIConfigMeta _sirenHeadAIConfig;
        
        private StateMachine _sirenHeadStateMachine;
        private SirenHeadSoundReproducer _soundReproducer;

        private Vector3 _startPosition;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => EnterMoveState();

        protected override void OnDestroyServerOnly()
        {
            DestroyTargetPoint();
            
            // LOCAL METHODS: -----------------------------

            void DestroyTargetPoint()
            {
                Transform targetPoint = _references.TargetPoint;
            
                if (targetPoint != null)
                    Destroy(targetPoint.gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Transform targetPoint = _references.TargetPoint;
            bool isTargetPointFound = targetPoint != null;

            if (!isTargetPointFound)
                return;

            Color gizmosColor = Gizmos.color;
            Gizmos.color = Color.red;
            
            Gizmos.DrawLine(transform.position, targetPoint.position);
            
            Gizmos.color = gizmosColor;
        }
#endif

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(SFXType sfxType)
        {
            PlaySoundLocal(sfxType);
            PlaySoundServerRPC(sfxType);
        }
        
        public void EnterIdleState() => ChangeState<IdleState>();

        public SirenHeadAIConfigMeta GetAIConfig() => _sirenHeadAIConfig;
        
        public References GetReferences() => _references;

        public Vector3 GetStartPosition() => _startPosition;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.SirenHead;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            _soundReproducer = new SirenHeadSoundReproducer(transform, _sirenHeadAIConfig);

        protected override void InitServerOnly()
        {
            ValidateTargetPoint();
            InitSystems();
            SetupStates();

            MonsterFootstepsSystem footstepsSystem =_references.FootstepsSystem;
            footstepsSystem.OnFootstepPerformedEvent += OnFootstepPerformed;

            // LOCAL METHODS: -----------------------------

            void ValidateTargetPoint()
            {
                Transform targetPoint = _references.TargetPoint;

                if (targetPoint != null)
                    targetPoint.SetParent(p: null);
                else
                    Log.PrintError(log: "<gb>Target Point</gb> <rb>not found</rb>!");
            }

            void InitSystems()
            {
                _sirenHeadStateMachine = new StateMachine();
                _startPosition = transform.position;

                Animator animator = _references.Animator;
                animator.SetFloat(id: AnimatorHashes.MotionSpeed, _sirenHeadAIConfig.AnimationSpeed);
            }

            void SetupStates()
            {
                IdleState idleState = new(sirenHeadEntity: this);
                MoveState moveState = new(sirenHeadEntity: this, _timeCycle);

                _sirenHeadStateMachine.AddState(idleState);
                _sirenHeadStateMachine.AddState(moveState);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(SFXType sfxType) =>
            _soundReproducer.PlaySound(sfxType);
        
        private void EnterMoveState() => ChangeState<MoveState>();

        private void ChangeState<TState>() where TState : IState =>
            _sirenHeadStateMachine.ChangeState<TState>();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRPC(SFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            PlaySoundClientRPC(sfxType, senderClientID);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(SFXType sfxType, ulong senderClientID)
        {
            bool isClientIDMatches = IsClientIDMatches(senderClientID);

            // Don't reproduce sound twice on sender.
            if (isClientIDMatches)
                return;

            PlaySoundLocal(sfxType);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnFootstepPerformed(string colliderTag) => PlaySound(SFXType.Footsteps);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button, DisableInEditorMode]
        private void DebugEnterIdleState() => EnterIdleState();
        
        [Button, DisableInEditorMode]
        private void DebugEnterMoveState() => EnterMoveState();
        
        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public class References
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Required]
            private Transform _targetPoint;

            [SerializeField, Required]
            private Animator _animator;

            [SerializeField, Required]
            private MonsterFootstepsSystem _footstepsSystem;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Transform TargetPoint => _targetPoint;
            public Animator Animator => _animator;
            public MonsterFootstepsSystem FootstepsSystem => _footstepsSystem;
        }
    }
}