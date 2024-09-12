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
        private enum Behaviour
        {
            None = 0,
            Hide = 1,
            Runaway = 2
        }

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

        private Behaviour _behaviour;
        private Vector3 _lastNoisePosition;
        private float _retreatingTimeLeft;
        private bool _isRetreating;
        private bool _isCheckEnabled;
        private bool _isPlayersNearby;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            if (_isCheckEnabled)
                return;

            _playersCheckRoutine.GetRoutineEvent += CheckForNearbyPlayersCO;

            _isCheckEnabled = true;
            _playersCheckRoutine.Start();
        }

        public void Tick()
        {
            _retreatingTimeLeft -= Time.deltaTime;

            if (_retreatingTimeLeft < 0f)
                _isRetreating = false;
        }

        public void Stop()
        {
            _playersCheckRoutine.GetRoutineEvent -= CheckForNearbyPlayersCO;

            _isCheckEnabled = false;
            _playersCheckRoutine.Stop();
        }

        public void DetectNoise(Vector3 noisePosition, float noiseLoudness)
        {
            if (!IsLoudEnough(noiseLoudness))
                return;

            bool isStateFound = _mushroomEntity.TryGetCurrentState(out IState state);

            if (!isStateFound)
                return;

            if (state is RunawayState or HidingState)
                return;
                
            StartRetreatingTimer();
            EnterRunawayState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FindInterestTargets()
        {
            _interestTargets.Clear();
            _isPlayersNearby = false;
            _behaviour = Behaviour.None;

            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            float visionRange = _suspicionSystemConfig.VisionRange;
            float interestAfterPlayerAfk = _suspicionSystemConfig.InterestAfterPlayerAfk;
            float distanceToHide = _suspicionSystemConfig.DistanceToHide;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                if (!IsPlayerEntityValid(playerEntity, out float distanceToPlayer))
                    continue;

                _isPlayersNearby = true;

                bool isAfkTimeValid = playerEntity.TimeSinceAfk >= interestAfterPlayerAfk;

                if (!isAfkTimeValid)
                {
                    _behaviour = distanceToPlayer <= distanceToHide ? Behaviour.Hide : Behaviour.Runaway;
                    continue;
                }

                _interestTargets.Add(playerEntity);
            }

            // LOCAL METHODS: -----------------------------

            bool IsPlayerEntityValid(PlayerEntity playerEntity, out float distance)
            {
                bool isValid = playerEntity != null && !playerEntity.IsDead();
                distance = 0f;

                if (!isValid)
                    return false;

                distance = Vector3.Distance(a: _transform.position, b: playerEntity.transform.position);
                bool isDistanceValid = distance <= visionRange;
                return isDistanceValid;
            }
        }

        private void CheckInterestTarget()
        {
            if (!TryGetCurrentState(out IState state))
                return;

            if (CheckHiding())
                return;

            if (CheckRunaway())
                return;

            int targetsAmount = _interestTargets.Count;

            if (targetsAmount == 0)
                ZeroTargetsLogic();
            else
                HaveTargetsLogic();

            // LOCAL METHODS: -----------------------------

            bool TryGetCurrentState(out IState result) =>
                _mushroomEntity.TryGetCurrentState(out result);

            bool CheckHiding()
            {
                if (_behaviour != Behaviour.Hide)
                    return false;

                if (state is not HidingState)
                    EnterHidingState();

                return true;
            }

            bool CheckRunaway()
            {
                if (_behaviour != Behaviour.Runaway)
                    return false;

                if (state is not RunawayState)
                {
                    StartRetreatingTimer();
                    EnterRunawayState();
                }

                return true;
            }

            void ZeroTargetsLogic()
            {
                if (_isPlayersNearby)
                    return;

                if (state is HidingState)
                {
                    EnterIdleState();
                    return;
                }

                if (state is RunawayState && !_isRetreating)
                {
                    EnterIdleState();
                    return;
                }

                if (state is MoveToInterestTargetState)
                    EnterIdleState();
            }

            void HaveTargetsLogic()
            {
                int randomIndex = Random.Range(0, targetsAmount);
                PlayerEntity interestTarget = _interestTargets[randomIndex];

                _mushroomEntity.SetInterestTarget(interestTarget);

                if (state is MoveToInterestTargetState)
                    return;

                if (state is LookAtInterestTargetState)
                    return;

                EnterMoveToInterestTargetState();
            }
        }

        private void StartRetreatingTimer()
        {
            _isRetreating = true;
            _retreatingTimeLeft = _suspicionSystemConfig.RetreatingTime;
        }

        private void EnterIdleState() =>
            _mushroomEntity.EnterIdleState();

        private void EnterHidingState() =>
            _mushroomEntity.EnterHidingState();

        private void EnterRunawayState() =>
            _mushroomEntity.EnterRunawayState();

        private void EnterMoveToInterestTargetState() =>
            _mushroomEntity.EnterMoveToInterestTargetState();

        private IEnumerator CheckForNearbyPlayersCO()
        {
            float interval = _suspicionSystemConfig.CheckNearbyPlayersInterval;
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

        private bool IsLoudEnough(float noiseLoudness)
        {
            float minLoudnessToReact = _suspicionSystemConfig.MinLoudnessToReact;
            bool isValid = noiseLoudness >= minLoudnessToReact;
            return isValid;
        }
    }
}