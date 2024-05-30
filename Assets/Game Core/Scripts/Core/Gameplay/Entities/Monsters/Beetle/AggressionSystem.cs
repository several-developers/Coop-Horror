using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Monsters.Beetle.States;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Beetle
{
    public class AggressionSystem
    {
        private enum AggressionStatus
        {
            Increase = 0,
            Decrease = 1
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AggressionSystem(BeetleEntity beetleEntity, BeetleAIConfigMeta beetleAIConfig)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleAIConfig;
            _transform = beetleEntity.transform;

            IEnumerator routine = TriggerCheckCO();
            beetleEntity.StartCoroutine(routine);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly Transform _transform;

        private AggressionStatus _aggressionStatus;
        private float _currentAggressionScale;
        private bool _triggerCheckEnabled;
        private bool _ignoreAggressionScale;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            UpdateAggressionScale();
            UpdateAggressionText();
            TryEnterScreamState();
        }

        public void ToggleTriggerCheckState(bool isEnabled) =>
            _triggerCheckEnabled = isEnabled;

        public void ToggleIgnoreAggressionScale(bool ignore) =>
            _ignoreAggressionScale = ignore;

        public void Reset()
        {
            _currentAggressionScale = 0f;
            _ignoreAggressionScale = true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateAggressionScale()
        {
            if (_ignoreAggressionScale)
                return;
            
            float deltaTime = Time.deltaTime;
            
            switch (_aggressionStatus)
            {
                case AggressionStatus.Increase:
                    float increaseSpeed = _beetleAIConfig.AggressionIncreaseSpeed;
                    float increasedValue = _currentAggressionScale + increaseSpeed * deltaTime;
                    _currentAggressionScale = Mathf.Min(a: increasedValue, b: _beetleAIConfig.AggressionScale);
                    break;

                case AggressionStatus.Decrease:
                    float decreaseSpeed = _beetleAIConfig.AggressionDecreaseSpeed;
                    float decreasedValue = _currentAggressionScale - decreaseSpeed * deltaTime;
                    _currentAggressionScale = Mathf.Max(a: decreasedValue, b: 0f);
                    break;
            }
        }

        private void UpdateAggressionText()
        {
            string text = $"Aggression: {_currentAggressionScale:F1}/{_beetleAIConfig.AggressionScale}";
            _beetleEntity.AggressionTMP.text = text;
        }

        private void CheckTriggerState()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            Vector3 beetlePosition = _transform.position;
            float triggerDistance = _beetleAIConfig.TriggerDistance;
            bool isAnyPlayerAtTriggerDistance = false;
            
            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                Vector3 playerPosition = playerEntity.transform.position;
                float distance = Vector3.Distance(a: beetlePosition, b: playerPosition);
                bool isTriggered = distance < triggerDistance;

                if (!isTriggered)
                    continue;

                _beetleEntity.SetTargetPlayer(playerEntity);
                isAnyPlayerAtTriggerDistance = true;
                break;
            }
            
            if (isAnyPlayerAtTriggerDistance)
            {
                SetAggressionStatus(AggressionStatus.Increase);
                TryEnterTriggerState();
            }
            else
            {
                SetAggressionStatus(AggressionStatus.Decrease);
                TryLeaveTriggerState();
            }
        }
        
        private void SetAggressionStatus(AggressionStatus aggressionStatus) =>
            _aggressionStatus = aggressionStatus;

        private void TryEnterTriggerState()
        {
            bool isStateFound = _beetleEntity.TryGetCurrentState(out IState state);
            bool isStateValid = isStateFound && state.GetType() != typeof(TriggerState);

            if (!isStateValid)
                return;
            
            EnterTriggerState();
        }

        private void TryEnterScreamState()
        {
            bool isMaxAggression = _currentAggressionScale >= _beetleAIConfig.AggressionScale;

            if (!isMaxAggression)
                return;

            bool isStateFound = _beetleEntity.TryGetCurrentState(out IState state);
            bool isStateValid = isStateFound && state.GetType() == typeof(TriggerState);

            if (!isStateValid)
                return;

            ToggleTriggerCheckState(isEnabled: false);
            EnterScreamState();
        }

        private void TryLeaveTriggerState()
        {
            bool isStateFound = _beetleEntity.TryGetCurrentState(out IState state);
            bool isStateValid = isStateFound && state.GetType() == typeof(TriggerState);

            if (!isStateValid)
                return;
            
            EnterIdleState();
        }

        private IEnumerator TriggerCheckCO()
        {
            while (true)
            {
                float checkInterval = _beetleAIConfig.TriggerDistanceCheckInterval;
                yield return new WaitForSeconds(checkInterval);

                if (!_triggerCheckEnabled)
                    continue;

                CheckTriggerState();
            }
        }

        private void EnterIdleState() =>
            _beetleEntity.EnterIdleState();

        private void EnterTriggerState() =>
            _beetleEntity.EnterTriggerState();

        private void EnterScreamState() =>
            _beetleEntity.EnterScreamState();
    }
}