using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.Player;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Zombies.States
{
    public class ChaseState : IEnterState, IExitState, ITickableState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ChaseState(ZombieEntity zombieEntity, IPlayerEntity playerEntity)
        {
            _zombieEntity = zombieEntity;
            _agent = zombieEntity.GetAgent();
            _animator = zombieEntity.GetAnimator();
            _transform = zombieEntity.transform;
            _playerTransform = playerEntity.GetTransform();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ZombieEntity _zombieEntity;
        private readonly NavMeshAgent _agent;
        private readonly Animator _animator;
        private readonly Transform _transform;
        private readonly Transform _playerTransform;

        private Vector2 _velocity;
        private Vector2 _smoothDeltaPosition;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            float attackDistance = _zombieEntity.GetAttackDistance();
            _agent.enabled = true;
            _agent.updatePosition = false;
            _agent.stoppingDistance = attackDistance;

            StartWithDelay();
        }

        public void Exit()
        {
            _animator.SetFloat(AnimatorHashes.Speed, 0);

            _zombieEntity.OnAnimatorMoveEvent -= OnAnimatorMove;
            _zombieEntity.OnMovementSpeedUpdatedEvent -= OnMovementSpeedUpdated;
        }

        public void Tick()
        {
            SetDestination();
            SynchronizedAnimatorAndAgent();

            if (!CanAttackTarget())
                return;

            EnterAttackState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void StartWithDelay()
        {
            float randomTime = Random.Range(0f, 0.2f);
            int delay = randomTime.ConvertToMilliseconds();
            
            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _zombieEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            UpdateAgentSpeed();
            UpdateMovementAnimationSpeed();

            _zombieEntity.OnAnimatorMoveEvent += OnAnimatorMove;
            _zombieEntity.OnMovementSpeedUpdatedEvent += OnMovementSpeedUpdated;
        }
        
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

            float velocityMagnitude = _velocity.magnitude;
            bool canMove = velocityMagnitude > 0.05f &&
                           _agent.remainingDistance > _agent.stoppingDistance;

            _animator.SetBool(id: AnimatorHashes.CanMove, canMove);
            _animator.SetFloat(id: AnimatorHashes.Speed, velocityMagnitude);

            float deltaMagnitude = worldDeltaPosition.magnitude;

            if (deltaMagnitude > _agent.radius / 2f)
                _transform.position = Vector3.Lerp(_animator.rootPosition, _agent.nextPosition, smooth);
        }

        private void UpdateAgentSpeed()
        {
            float movementSpeed = _zombieEntity.GetMovementSpeed();
            _agent.speed = movementSpeed;
        }

        private void UpdateMovementAnimationSpeed()
        {
            float inputMagnitude = _zombieEntity.GetMovementInputMagnitude();
            //float movementSpeed = _zombieEntity.GetMovementSpeed();

            //_animator.SetFloat(AnimatorHashes.Speed, movementSpeed);
            _animator.SetFloat(id: AnimatorHashes.MotionSpeed, inputMagnitude);
        }

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

        private void EnterAttackState() =>
            _zombieEntity.EnterAttackState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAnimatorMove()
        {
            Vector3 rootPosition = _animator.rootPosition;
            rootPosition.y = _agent.nextPosition.y;
            _transform.position = rootPosition;
            _agent.nextPosition = rootPosition;
        }

        private void OnMovementSpeedUpdated()
        {
            UpdateAgentSpeed();
            UpdateMovementAnimationSpeed();
        }
    }
}