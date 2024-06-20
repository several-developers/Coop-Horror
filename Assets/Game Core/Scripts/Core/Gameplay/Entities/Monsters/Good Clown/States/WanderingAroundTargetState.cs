using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.MovementLogics;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown.States
{
    public class WanderingAroundTargetState : IEnterState, ITickableState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingAroundTargetState(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _transform = goodClownEntity.transform;
            _agent = goodClownEntity.GetAgent();
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _wanderingMovementLogic = new WanderingMovementLogic(goodClownEntity.transform, _agent);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta _goodClownAIConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _wanderingMovementLogic.OnStuckEvent += OnStuck;
            _wanderingMovementLogic.OnArrivedEvent += OnArrived;
            _wanderingMovementLogic.GetRandomPositionEvent += GetRandomPosition;
            
            EnableAgent();
            SetWalkingAnimation();
            
            if (!_wanderingMovementLogic.TrySetDestinationPoint())
                EnterMoveToTargetState();
        }

        public void Tick()
        {
            _wanderingMovementLogic.Tick();
            UpdateAnimationMoveSpeed();
        }

        public void Exit()
        {
            _wanderingMovementLogic.OnStuckEvent -= OnStuck;
            _wanderingMovementLogic.OnArrivedEvent -= OnArrived;
            _wanderingMovementLogic.GetRandomPositionEvent -= GetRandomPosition;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = GetWanderingSpeed();
        }

        private void SetWalkingAnimation() =>
            _clownUtilities.SetWalkingAnimation();

        private void UpdateAnimationMoveSpeed() =>
            _clownUtilities.UpdateAnimationMoveSpeed();

        private async void SetNewDestinationPointWithDelay()
        {
            float minDelay = _goodClownAIConfig.WanderingMinDelay;
            float maxDelay = _goodClownAIConfig.WanderingMaxDelay;
            float delayInSeconds = Random.Range(minDelay, maxDelay);
            int delay = delayInSeconds.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _goodClownEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            _wanderingMovementLogic.TrySetDestinationPoint();
        }

        private void EnterMoveToTargetState() =>
            _goodClownEntity.EnterMoveToTargetState();

        private Vector3 GetRandomPosition()
        {
            float minDistance = _goodClownAIConfig.WanderingMinDistance;
            float maxDistance = _goodClownAIConfig.WanderingMaxDistance;
            float distance = Random.Range(minDistance, maxDistance);

            Vector2 circle = Random.insideUnitCircle;
            circle *= distance;
            
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;
            
            Vector3 circlePosition = new(x: circle.x, y: 0f, z: circle.y);
            Vector3 targetPosition = isTargetFound ? targetPlayer.transform.position : _transform.position;
            Vector3 newPosition = circlePosition + targetPosition;

            return newPosition;
        }
        
        private float GetWanderingSpeed()
        {
            float minSpeed = _goodClownAIConfig.WanderingMinSpeed;
            float maxSpeed = _goodClownAIConfig.WanderingMaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStuck() =>
            _wanderingMovementLogic.TrySetDestinationPoint();

        private void OnArrived() => SetNewDestinationPointWithDelay();
    }
}