using GameCore.Gameplay.Entities.Other;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Other;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Zombies.States
{
    public class AttackState : IEnterState, IExitState, ITickableState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackState(ZombieEntity zombieEntity, PlayerEntity playerEntity)
        {
            _zombieEntity = zombieEntity;
            _playerEntity = playerEntity;
            _agent = zombieEntity.GetAgent();
            _animationObserver = zombieEntity.GetAnimationObserver();
            _animator = zombieEntity.GetAnimator();
            _transform = zombieEntity.transform;
            _playerTransform = playerEntity.GetTransform();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ZombieEntity _zombieEntity;
        private readonly PlayerEntity _playerEntity;
        private readonly NavMeshAgent _agent;
        private readonly AnimationObserver _animationObserver;
        private readonly Animator _animator;
        private readonly Transform _transform;
        private readonly Transform _playerTransform;

        private Vector2 _velocity;
        private Vector2 _smoothDeltaPosition;
        private float _attackCdLeft;
        private bool _isAttackOnCd;
        private bool _isAttacking;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            _attackCdLeft = 0f;
            _agent.updatePosition = false;
            
            _animationObserver.OnAttackEvent += OnAttack;
            _animationObserver.OnAttackFinishedEvent += OnAttackFinished;

            _zombieEntity.OnAnimatorMoveEvent += OnAnimatorMove;
        }

        public void Exit()
        {
            _animationObserver.OnAttackEvent -= OnAttack;
            _animationObserver.OnAttackFinishedEvent -= OnAttackFinished;
            
            _zombieEntity.OnAnimatorMoveEvent -= OnAnimatorMove;
        }

        public void Tick()
        {
            SetDestination();
            SynchronizedAnimatorAndAgent();
            
            if (_isAttacking)
                return;

            if (IsPlayerDead())
            {
                EnterIdleState();
                return;
            }
            
            _attackCdLeft -= Time.deltaTime;

            if (_attackCdLeft > 0f)
                return;

            if (!CanAttackTarget())
            {
                EnterChaseState();
                return;
            }

            _attackCdLeft = _zombieEntity.GetAttackCd();
            
            StartAttackAnimation();
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDestination() =>
            _agent.destination = _playerTransform.position;
        
        private void SynchronizedAnimatorAndAgent()
        {
            Vector3 worldDeltaPosition = _agent.nextPosition - _transform.position;
            worldDeltaPosition.y = 0;

            float dx = Vector3.Dot(_transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(_transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new(dx, dy);

            float smooth = Mathf.Min(1f, Time.deltaTime / 0.1f);
            _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPosition, smooth);

            _velocity = _smoothDeltaPosition / Time.deltaTime;

            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                float t = _agent.remainingDistance / _agent.stoppingDistance;
                _velocity = Vector2.Lerp(Vector2.zero, _velocity, t);
            }

            float velocityMagnitude = _isAttacking ? 0 : _velocity.magnitude;
            bool canMove = velocityMagnitude > 0.15f &&
                           _agent.remainingDistance > _agent.stoppingDistance;

            _animator.SetBool(id: AnimatorHashes.CanMove, canMove);
            _animator.SetFloat(id: AnimatorHashes.Speed, velocityMagnitude);

            float deltaMagnitude = worldDeltaPosition.magnitude;

            if (deltaMagnitude > _agent.radius / 2f)
                _transform.position = Vector3.Lerp(_animator.rootPosition, _agent.nextPosition, smooth);
        }
        
        private void StartAttackAnimation()
        {
            _isAttacking = true;
            
            _animator.SetTrigger(id: AnimatorHashes.Attack);
        }

        private void PerformAttack()
        {
            if (IsPlayerDead())
                return;
            
        }

        private bool IsPlayerDead() =>
            _playerEntity.IsDead();
        
        private bool CanAttackTarget()
        {
            float attackDistance = _zombieEntity.GetAttackDistance();
            float distance = _agent.remainingDistance;
            
            if (float.IsInfinity(distance))
                return false;
            
            if (distance < 0.2f)
                return false;

            if (distance <= attackDistance)
                return true;

            return false;
        }

        private void EnterIdleState() =>
            _zombieEntity.EnterIdleState();

        private void EnterChaseState() =>
            _zombieEntity.EnterChaseState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAttack() => PerformAttack();

        private void OnAttackFinished() =>
            _isAttacking = false;

        private void OnAnimatorMove()
        {
            Vector3 rootPosition = _animator.rootPosition;
            rootPosition.y = _agent.nextPosition.y;
            _transform.position = rootPosition;
            _agent.nextPosition = rootPosition;
        }
    }
}