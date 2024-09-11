using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.Mushroom.States;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class SuspicionSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SuspicionSystem(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();

            _mushroomEntity = mushroomEntity;
            _suspicionSystemConfig = mushroomAIConfig.GetSuspicionSystemConfig();
            _transform = mushroomEntity.transform;
            _playersCheckRoutine = new CoroutineHelper(mushroomEntity);
            _interestTargets = new List<PlayerEntity>(capacity: 4);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.SuspicionSystemConfig _suspicionSystemConfig;
        private readonly Transform _transform;
        private readonly CoroutineHelper _playersCheckRoutine;
        private readonly List<PlayerEntity> _interestTargets;

        private float _retreatingTimeLeft;
        private bool _isRetreating;
        private bool _isCheckEnabled;
        private bool _isPlayersNearby;
        private bool _shouldHide;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            if (_isCheckEnabled)
                return;

            _playersCheckRoutine.GetRoutineEvent += CheckForNearbyPlayersCO;

            _isCheckEnabled = true;
            _playersCheckRoutine.Start();
        }

        public void Stop()
        {
            _playersCheckRoutine.GetRoutineEvent -= CheckForNearbyPlayersCO;

            _isCheckEnabled = false;
            _playersCheckRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FindInterestTargets()
        {
            _interestTargets.Clear();
            _isPlayersNearby = false;
            _shouldHide = false;

            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            float visionRange = _suspicionSystemConfig.VisionRange;
            float interestAfterPlayerAfk = _suspicionSystemConfig.InterestAfterPlayerAfk;
            float distanceToHide = _suspicionSystemConfig.DistanceToHide;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isValid = playerEntity != null && !playerEntity.IsDead();

                if (!isValid)
                    continue;

                float distanceToPlayer = Vector3.Distance(a: _transform.position, b: playerEntity.transform.position);
                bool isDistanceValid = distanceToPlayer <= visionRange;

                if (!isDistanceValid)
                    continue;

                _isPlayersNearby = true;

                bool isAfkTimeValid = playerEntity.TimeSinceAfk >= interestAfterPlayerAfk;

                if (!isAfkTimeValid)
                {
                    if (distanceToPlayer <= distanceToHide)
                        _shouldHide = true;
                    
                    continue;
                }

                _interestTargets.Add(playerEntity);
            }
        }

        private void CheckInterestTarget()
        {
            if (!TryGetCurrentState(out IState state))
                return;

            if (CheckHiding())
                return;

            int targetsAmount = _interestTargets.Count;
            bool isStateMoveToInterestTarget = state is MoveToInterestTargetState;

            if (targetsAmount == 0)
                ZeroTargetsLogic();
            else
                HaveTargetsLogic();

            // LOCAL METHODS: -----------------------------

            bool TryGetCurrentState(out IState result) =>
                _mushroomEntity.TryGetCurrentState(out result);

            bool CheckHiding()
            {
                if (!_shouldHide)
                    return false;

                if (state is not HidingState)
                    EnterHidingState();

                return true;
            }

            void ZeroTargetsLogic()
            {
                if (!_isPlayersNearby)
                    return;

                if (state is HidingState)
                {
                    EnterIdleState();
                    return;
                }
                
                if (isStateMoveToInterestTarget)
                    EnterIdleState();
            }

            void HaveTargetsLogic()
            {
                int randomIndex = Random.Range(0, targetsAmount);
                PlayerEntity interestTarget = _interestTargets[randomIndex];

                _mushroomEntity.SetInterestTarget(interestTarget);

                if (isStateMoveToInterestTarget)
                    return;

                if (state is LookAtInterestTargetState)
                    return;

                EnterMoveToInterestTargetState();
            }

            void HideOrRunaway()
            {
                
            }
        }

        private void EnterIdleState() =>
            _mushroomEntity.EnterIdleState();

        private void EnterHidingState() =>
            _mushroomEntity.EnterHidingState();

        private void EnterRunawayState()
        {
            // EnterIdleState();
        }

        private void EnterMoveToInterestTargetState() =>
            _mushroomEntity.EnterMoveToInterestTargetState();

        private IEnumerator CheckForNearbyPlayersCO()
        {
            float interval = _suspicionSystemConfig.CheckForNearbyPlayersInterval;
            var waitForSeconds = new WaitForSeconds(interval);

            while (true)
            {
                if (!_isRetreating)
                {
                    FindInterestTargets();
                    CheckInterestTarget();
                }

                yield return waitForSeconds;
            }
        }
    }
}