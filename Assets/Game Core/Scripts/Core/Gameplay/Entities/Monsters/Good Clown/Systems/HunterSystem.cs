using System.Collections;
using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class HunterSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HunterSystem(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _transform = goodClownEntity.transform;
            _checkForHuntingRoutine = new CoroutineHelper(goodClownEntity);
            _checkForTransformationRoutine = new CoroutineHelper(goodClownEntity);

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _hunterSystemConfig = goodClownAIConfig.HunterSystemConfig;

            _checkForHuntingRoutine.GetRoutineEvent += CheckForHuntingCO;
            _checkForTransformationRoutine.GetRoutineEvent += CheckForTransformationCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.HunterSystemSettings _hunterSystemConfig;
        private readonly Transform _transform;
        private readonly CoroutineHelper _checkForHuntingRoutine;
        private readonly CoroutineHelper _checkForTransformationRoutine;

        private Coroutine _checkForHuntingCO;
        private float _currentHuntingTime;
        private bool _isHuntingStarted;
        private bool _isHuntingOver;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            Stop();
            _checkForHuntingRoutine.Start();
        }

        public void Tick()
        {
            if (!_isHuntingStarted || _isHuntingOver)
                return;
            
            _currentHuntingTime += Time.deltaTime;
            _isHuntingOver = _currentHuntingTime >= _hunterSystemConfig.HuntingDuration;

            if (!_isHuntingOver)
                return;
            
            _checkForTransformationRoutine.Start();
        }
        
        public void Stop() =>
            _checkForHuntingRoutine.Stop();

        public void StopHuntingTimer()
        {
            _isHuntingStarted = false;
            _isHuntingOver = false;
            _currentHuntingTime = 0f;
        }

        public void Destroy()
        {
            _checkForHuntingRoutine.GetRoutineEvent -= CheckForHuntingCO;
            _checkForTransformationRoutine.GetRoutineEvent -= CheckForTransformationCO;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckForHunting()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
                return;

            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 thisPosition = _transform.position;
            float distance = Vector3.Distance(a: targetPosition, b: thisPosition);
            bool isDistanceValid = distance <= _hunterSystemConfig.TargetDistanceForHunt;

            if (!isDistanceValid)
                return;

            bool isOtherPlayersNear = IsOtherPlayersNear(targetPlayer);

            if (isOtherPlayersNear)
                return;

            Stop();
            StartHuntingTimer();
            EnterHuntingIdleState();
        }

        private void CheckForTransformation()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
                return;

            Transform target = targetPlayer.transform;
            float lookDirectionDot = EntitiesUtilities.GetLookDirectionDot(owner: _transform, target);
            bool canTransform = lookDirectionDot < 0.3f;

            // string log = Log.HandleLog($"Look Value: <gb>{lookDirectionDot}</gb>");
            // Debug.Log(log);

            if (!canTransform)
                return;
            
            _checkForTransformationRoutine.Stop();
            EnterRespawnAsEvilClownState();
        }
        
        private void StartHuntingTimer()
        {
            _isHuntingStarted = true;
            _isHuntingOver = false;
            _currentHuntingTime = 0f;
        }

        private void EnterHuntingIdleState() =>
            _goodClownEntity.EnterHuntingIdleState();

        private void EnterRespawnAsEvilClownState() =>
            _goodClownEntity.EnterRespawnAsEvilClownState();

        private IEnumerator CheckForHuntingCO()
        {
            while (true)
            {
                float checkInterval = _hunterSystemConfig.HuntingCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                CheckForHunting();
            }
        }

        private IEnumerator CheckForTransformationCO()
        {
            while (true)
            {
                float checkInterval = _hunterSystemConfig.TransformationCheckInterval;
                yield return new WaitForSeconds(checkInterval);

                CheckForTransformation();
            }
        }

        private bool IsOtherPlayersNear(PlayerEntity targetPlayer)
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            Vector3 thisPosition = _transform.position;
            bool isOtherPlayersNear = false;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                bool isTargetPlayer = playerEntity == targetPlayer;

                if (isTargetPlayer)
                    continue;

                Vector3 playerPosition = playerEntity.transform.position;
                float distance = Vector3.Distance(a: playerPosition, b: thisPosition);
                bool isDistanceValid = distance > _hunterSystemConfig.DistanceToOtherPlayersForHunt;

                if (isDistanceValid)
                    continue;

                isOtherPlayersNear = true;
                break;
            }

            if (!isOtherPlayersNear && _hunterSystemConfig.DisableHunting)
                isOtherPlayersNear = true;

            return isOtherPlayersNear;
        }
    }
}