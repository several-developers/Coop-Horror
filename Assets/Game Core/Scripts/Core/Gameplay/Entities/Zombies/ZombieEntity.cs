using System;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Zombies.States;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GameCore.Gameplay.Entities.Zombies
{
    public class ZombieEntity : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(PlayerEntity playerEntity)
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
        
        [SerializeField, Required, Space(5)]
        private GameObject[] _hitBoxes;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnAnimatorMoveEvent;
        public event Action OnMovementSpeedUpdatedEvent;

        private StateMachine _zombieStateMachine;
        private PlayerEntity _playerEntity;

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
            
            CreateAndSetupStateMachine();
        }

        public void DisableHitBoxes()
        {
            foreach (GameObject hitBox in _hitBoxes)
                hitBox.SetActive(false);
        }

        public void SendDeathEvent()
        {
        }

        public void EnterIdleState() => EnterState<IdleState>();

        public void EnterChaseState() => EnterState<ChaseState>();

        public void EnterAttackState() => EnterState<AttackState>();

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
            ChaseState chaseState = new(zombieEntity: this, _playerEntity);
            AttackState attackState = new(zombieEntity: this, _playerEntity);
            DeathState deathState = new(zombieEntity: this, _aimTarget);

            _zombieStateMachine.AddState(idleState);
            _zombieStateMachine.AddState(chaseState);
            _zombieStateMachine.AddState(attackState);
            _zombieStateMachine.AddState(deathState);
        }

        private void EnterState<TState>() where TState : IState =>
            _zombieStateMachine.ChangeState<TState>();

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