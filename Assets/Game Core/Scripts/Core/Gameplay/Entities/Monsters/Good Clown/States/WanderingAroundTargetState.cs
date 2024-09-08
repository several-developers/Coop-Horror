using System.Collections;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Movement;
using GameCore.Gameplay.Systems.Utilities;
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
            _transform = goodClownEntity.transform;
            _agent = goodClownEntity.GetAgent();
            _clownUtilities = goodClownEntity.GetClownUtilities();
            _wanderingDistanceBreakCheckRoutine = new CoroutineHelper(goodClownEntity);
            _wanderingMovementLogic = new WanderingMovementLogic(goodClownEntity.transform, _agent);
            
            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _wanderingConfig = goodClownAIConfig.WanderingConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.WanderingSettings _wanderingConfig;
        private readonly Transform _transform;
        private readonly NavMeshAgent _agent;
        private readonly GoodClownUtilities _clownUtilities;
        private readonly CoroutineHelper _wanderingDistanceBreakCheckRoutine;
        private readonly WanderingMovementLogic _wanderingMovementLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _wanderingDistanceBreakCheckRoutine.GetRoutineEvent += WanderingDistanceBreakCheckCO;
            
            _wanderingMovementLogic.OnStuckEvent += OnStuck;
            _wanderingMovementLogic.OnArrivedEvent += OnArrived;
            _wanderingMovementLogic.GetRandomPositionEvent += GetRandomPosition;
            
            EnableAgent();
            _wanderingDistanceBreakCheckRoutine.Start();
            
            if (!_wanderingMovementLogic.TrySetDestinationPoint())
                EnterFollowTargetState();
        }

        public void Tick()
        {
            _wanderingMovementLogic.Tick();
            UpdateAnimationMoveSpeed();
        }

        public void Exit()
        {
            _wanderingDistanceBreakCheckRoutine.GetRoutineEvent -= WanderingDistanceBreakCheckCO;
            
            _wanderingMovementLogic.OnStuckEvent -= OnStuck;
            _wanderingMovementLogic.OnArrivedEvent -= OnArrived;
            _wanderingMovementLogic.GetRandomPositionEvent -= GetRandomPosition;
            
            _wanderingDistanceBreakCheckRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableAgent()
        {
            _agent.enabled = true;
            _agent.speed = GetWanderingSpeed();
        }

        private void UpdateAnimationMoveSpeed() =>
            _clownUtilities.UpdateAnimationMoveSpeed();

        private async UniTaskVoid SetNewDestinationPointWithDelay()
        {
            float minDelay = _wanderingConfig.MinDelay;
            float maxDelay = _wanderingConfig.MaxDelay;
            float delayInSeconds = Random.Range(minDelay, maxDelay);
            int delay = delayInSeconds.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: _goodClownEntity.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            _wanderingMovementLogic.TrySetDestinationPoint();
        }

        private void CheckWanderingBreakDistance()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
            {
                EnterSearchForTargetState();
                return;
            }

            Vector3 playerPosition = targetPlayer.transform.position;
            Vector3 thisPosition = _transform.position;
            float distance = Vector3.Distance(a: playerPosition, b: thisPosition);
            bool breakWandering = distance >= _wanderingConfig.DistanceToBreakWandering;

            if (!breakWandering)
                return;

            EnterFollowTargetState();
        }

        private void EnterSearchForTargetState() =>
            _goodClownEntity.EnterSearchForTargetState();
        
        private void EnterFollowTargetState() =>
            _goodClownEntity.EnterFollowTargetState();

        private IEnumerator WanderingDistanceBreakCheckCO()
        {
            while (true)
            {
                float delay = _wanderingConfig.WanderingDistanceBreakCheckInterval;
                yield return new WaitForSeconds(delay);

                CheckWanderingBreakDistance();
            }
        }
        
        private Vector3 GetRandomPosition()
        {
            float minDistance = _wanderingConfig.MinDistance;
            float maxDistance = _wanderingConfig.MaxDistance;
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
            float minSpeed = _wanderingConfig.MinSpeed;
            float maxSpeed = _wanderingConfig.MaxSpeed;
            float speed = Random.Range(minSpeed, maxSpeed);
            return speed;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStuck() =>
            _wanderingMovementLogic.TrySetDestinationPoint();

        private void OnArrived() =>
            SetNewDestinationPointWithDelay().Forget();
    }
}