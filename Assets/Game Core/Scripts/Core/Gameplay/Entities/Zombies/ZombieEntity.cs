using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.Other;
using GameCore.Gameplay.Entities.Other.DamageReceivers;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Zombies.States;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GameCore.Gameplay.Entities.Zombies
{
    public class ZombieEntity : MonoBehaviour, ITarget
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IPlayerEntity playerEntity)
        {
            _playerEntity = playerEntity;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _movementSpeed = 1f;

        [SerializeField, Min(0)]
        private float _crawlingSpeed = 0.5f;

        [SerializeField, Min(0)]
        private float _movementInputMagnitude = 1.4f;

        [SerializeField, Min(0)]
        private float _crawlingInputMagnitude = 0.9f;

        [SerializeField, Min(0)]
        private float _attackDistance = 5f;

        [SerializeField, Min(0)]
        private float _attackCd = 1.2f;

        [SerializeField]
        private bool _idleStateAtStart = true;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        [SerializeField, Required]
        private NavMeshAgent _agent;

        [SerializeField, Required]
        private CapsuleCollider _aimTarget;
        
        [SerializeField, Required]
        private RagdollEnabler _ragdollEnabler;

        [SerializeField, Required, Space(5)]
        private GameObject[] _hitBoxes;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAnimatorMoveEvent;
        public event Action OnMovementSpeedUpdatedEvent;
        public event Action<ITarget> OnDeathEvent;

        private StateMachine _zombieStateMachine;
        private IHealthSystem _healthSystem;
        private IPlayerEntity _playerEntity;
        private ZombieCrawlingHandler _zombieCrawlingHandler;

        private float _currentMovementSpeed;
        private float _currentInputMagnitude;
        private bool _isDead;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            Setup();

            if (_idleStateAtStart)
                EnterIdleState();
            else
                EnterChaseState();
        }

        private void Update()
        {
            if (_isDead)
                return;

            _zombieStateMachine.Tick();
        }

        private void OnAnimatorMove()
        {
            if (_isDead)
                return;

            OnAnimatorMoveEvent?.Invoke();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup()
        {
            _currentMovementSpeed = _movementSpeed;
            _currentInputMagnitude = _movementInputMagnitude;

            _zombieCrawlingHandler = new ZombieCrawlingHandler(_healthSystem, _animator);
            
            CreateAndSetupStateMachine();

            _healthSystem.OnHealthUpdatedEvent += OnHealthUpdated;
            _healthSystem.OnDeathEvent += OnDeath;

            _zombieCrawlingHandler.OnCrawlingEnabledEvent += OnCrawlingEnabled;
        }

        public void DisableHitBoxes()
        {
            foreach (GameObject hitBox in _hitBoxes)
                hitBox.SetActive(false);
        }

        public void SendDeathEvent() =>
            OnDeathEvent?.Invoke(this);

        public void EnterIdleState() => EnterState<IdleState>();

        public void EnterScreamState() => EnterState<ScreamState>();

        public void EnterChaseState() => EnterState<ChaseState>();

        public void EnterAttackState() => EnterState<AttackState>();

        public void EnterChaseStateWithChance()
        {
            if (GlobalUtilities.IsRandomSuccessful(chance: 10))
                EnterScreamState();
            else
                EnterChaseState();
        }

        public async void ActivateChase(float delay)
        {
            int delayInMilliseconds = delay.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delayInMilliseconds, cancellationToken: gameObject.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            EnterChaseState();
        }

        public async void ActivateChaseWithChance(float delay)
        {
            int delayInMilliseconds = delay.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delayInMilliseconds, cancellationToken: gameObject.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            if (GlobalUtilities.IsRandomSuccessful(chance: 10))
                EnterScreamState();
            else
                EnterChaseState();
        }

        public Transform GetTransform() => transform;

        public Animator GetAnimator() => _animator;

        public AnimationObserver GetAnimationObserver() => _animationObserver;

        public NavMeshAgent GetAgent() => _agent;

        public float GetMovementSpeed() => _currentMovementSpeed;

        public float GetMovementInputMagnitude() => _currentInputMagnitude;

        public float GetAttackDistance() => _attackDistance;

        public float GetAttackCd() => _attackCd;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateAndSetupStateMachine()
        {
            _zombieStateMachine = new StateMachine();

            IdleState idleState = new(zombieEntity: this);
            ScreamState screamState = new(zombieEntity: this);
            ChaseState chaseState = new(zombieEntity: this, _playerEntity);
            AttackState attackState = new(zombieEntity: this, _playerEntity);
            DeathState deathState = new(zombieEntity: this, _ragdollEnabler, _aimTarget);

            _zombieStateMachine.AddState(idleState);
            _zombieStateMachine.AddState(screamState);
            _zombieStateMachine.AddState(chaseState);
            _zombieStateMachine.AddState(attackState);
            _zombieStateMachine.AddState(deathState);
        }

        private void TryEnterScreamStateFromIdleState()
        {
            if (!_zombieStateMachine.TryGetCurrentState(out IState state))
                return;

            bool isIdleState = state is IdleState;

            if (!isIdleState)
                return;

            EnterScreamState();
        }

        private void EnterState<TState>() where TState : IState =>
            _zombieStateMachine.ChangeState<TState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHealthUpdated(HealthStaticData healthData)
        {
            //string log = Log.HandleLog($"Health: <gb>({healthData.CurrentHealth:F0}/{healthData.MaxHealth:F0})</gb>.");
            //Debug.Log(log);

            _animator.SetTrigger(id: AnimatorHashes.HitReaction);

            TryEnterScreamStateFromIdleState();
        }

        private void OnCrawlingEnabled()
        {
            _currentMovementSpeed = _crawlingSpeed;
            _currentInputMagnitude = _crawlingInputMagnitude;

            OnMovementSpeedUpdatedEvent?.Invoke();
        }

        private void OnDeath()
        {
            _healthSystem.OnHealthUpdatedEvent -= OnHealthUpdated;
            _healthSystem.OnDeathEvent -= OnDeath;

            _isDead = true;

            EnterState<DeathState>();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugEnterIdleState() => EnterIdleState();

        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugEnterChaseState() => EnterChaseState();

        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugEnterDeathState() => EnterState<DeathState>();
    }
}