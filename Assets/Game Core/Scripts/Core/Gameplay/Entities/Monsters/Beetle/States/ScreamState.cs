using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class ScreamState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ScreamState(BeetleEntity beetleEntity)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
            _transform = beetleEntity.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;

        private bool _isScreamInCooldown;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            TryScream();
            EnterChaseState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryScream()
        {
            if (_isScreamInCooldown)
                return;

            Scream();
            StartScreamCooldown();
        }

        private void Scream()
        {
            IReadOnlyList<BeetleEntity> allBeetles = BeetleEntity.GetAllBeetles();

            foreach (BeetleEntity beetleEntity in allBeetles)
            {
                bool isOwner = beetleEntity == _beetleEntity;

                if (isOwner)
                    continue;

                Vector3 targetBeetlePosition = beetleEntity.transform.position;
                Vector3 beetlePosition = _transform.position;
                float distance = Vector3.Distance(a: beetlePosition, b: targetBeetlePosition);
                bool isTooFar = distance > _beetleAIConfig.ScreamDistance;

                if (isTooFar)
                    continue;

                bool isStateFound = beetleEntity.TryGetCurrentState(out IState state);

                if (!isStateFound)
                    continue;

                Type stateType = state.GetType();
                bool isStateValid = stateType == typeof(IdleState) || stateType == typeof(WanderingState);

                if (!isStateValid)
                    continue;

                PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();
                AggressionSystem aggressionSystem = beetleEntity.GetAggressionSystem();
                
                beetleEntity.SetTargetPlayer(targetPlayer);
                beetleEntity.EnterChaseState();
                aggressionSystem.ToggleTriggerCheckState(isEnabled: false);
                aggressionSystem.SetAggressionStatus(AggressionSystem.AggressionStatus.Increase);
                aggressionSystem.SetMaxAggressionScale();
            }
        }

        private void StartScreamCooldown()
        {
            IEnumerator routine = ScreamCooldownCO();
            _beetleEntity.StartCoroutine(routine);
        }

        private IEnumerator ScreamCooldownCO()
        {
            _isScreamInCooldown = true;

            float delay = _beetleAIConfig.ScreamCooldown;
            yield return new WaitForSeconds(delay);

            _isScreamInCooldown = false;
        }

        private void EnterChaseState() =>
            _beetleEntity.EnterChaseState();
    }
}