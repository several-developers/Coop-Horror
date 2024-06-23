using System;
using System.Collections;
using GameCore.Gameplay.EntitiesSystems.Utilities;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.MovementLogics
{
    public abstract class MoveToFireExitLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        protected MoveToFireExitLogic(MonoBehaviour coroutineRunner, NavMeshAgent agent, ILevelProvider levelProvider)
        {
            _agent = agent;
            _transform = coroutineRunner.transform;
            _distanceCheckRoutine = new CoroutineHelper(coroutineRunner);
            LevelProvider = levelProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnFireExitNotFoundEvent = delegate { };
        public event Action<FireExit> OnInteractWithFireExitEvent = delegate { };
        public event Func<float> GetFireExitDistanceCheckIntervalEvent = () => 0.25f;
        public event Func<float> GetFireExitInteractionDistanceEvent = () => 0.1f;

        protected readonly ILevelProvider LevelProvider;

        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly CoroutineHelper _distanceCheckRoutine;
        
        private FireExit _fireExit;
        private Vector3 _destinationPoint;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            _distanceCheckRoutine.GetRoutineEvent += DistanceCheckCO;
            
            _distanceCheckRoutine.Start();
            SetDestinationPoint();
        }

        public void Stop()
        {
            _distanceCheckRoutine.GetRoutineEvent -= DistanceCheckCO;
            
            _distanceCheckRoutine.Stop();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract bool TryGetFireExit(out FireExit fireExit);
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDestinationPoint()
        {
            bool isFireExitFound = TryGetFireExit(out _fireExit);

            if (!isFireExitFound)
            {
                OnFireExitNotFoundEvent.Invoke();
                return;
            }

            Transform teleportPoint = _fireExit.GetTeleportPoint();
            _destinationPoint = teleportPoint.position;
            _agent.destination = _destinationPoint;
        }

        private void CheckDistanceToFireExit()
        {
            Vector3 thisPosition = _transform.position;
            float distance = Vector3.Distance(a: thisPosition, b: _destinationPoint);
            bool canInteract = distance <= GetFireExitInteractionDistanceEvent.Invoke();

            if (!canInteract)
                return;

            OnInteractWithFireExitEvent.Invoke(_fireExit);
        }

        private IEnumerator DistanceCheckCO()
        {
            while (true)
            {
                float checkInterval = GetFireExitDistanceCheckIntervalEvent.Invoke();
                yield return new WaitForSeconds(checkInterval);

                CheckDistanceToFireExit();
            }
        }
    }
}