using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
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

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _hunterSystemConfig = goodClownAIConfig.HunterSystemConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta.HunterSystemSettings _hunterSystemConfig;
        private readonly Transform _transform;

        private Coroutine _checkForHuntingCO;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            Stop();
            StartCheckForHunting();
        }

        public void Stop() => StopCheckForHunting();

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
            EnterHuntingIdleState();
        }

        private void StartCheckForHunting()
        {
            IEnumerator routine = CheckForHuntingCO();
            _checkForHuntingCO = _goodClownEntity.StartCoroutine(routine);
        }

        private void StopCheckForHunting()
        {
            if (_checkForHuntingCO == null)
                return;
            
            _goodClownEntity.StopCoroutine(_checkForHuntingCO);
        }

        private void EnterHuntingIdleState() =>
            _goodClownEntity.EnterHuntingIdleState();

        private IEnumerator CheckForHuntingCO()
        {
            while (true)
            {
                float checkInterval = _hunterSystemConfig.HuntingCheckInterval;
                yield return new WaitForSeconds(checkInterval);
                
                CheckForHunting();
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