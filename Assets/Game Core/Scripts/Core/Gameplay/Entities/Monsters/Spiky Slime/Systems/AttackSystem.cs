using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class AttackSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackSystem(SpikySlimeEntity spikySlimeEntity)
        {
            AnimationController animationController = spikySlimeEntity.GetAnimationController();
            SpikySlimeAIConfigMeta spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();

            _spikySlimeEntity = spikySlimeEntity;
            _attackSystemConfig = spikySlimeAIConfig.GetAttackSystemConfig();
            _references = spikySlimeEntity.GetReferences();
            _aggressionSystem = spikySlimeEntity.GetAggressionSystem();
            _attackTrigger = _references.AttackTrigger;
            _attackRoutine = new CoroutineHelper(spikySlimeEntity);
            _playersQueues = new List<PlayerQueue>();

            _attackRoutine.GetRoutineEvent += AttackCO;

            _attackTrigger.OnTriggerEvent += OnPlayerEnterAttackTrigger;

            animationController.OnAttackAnimationEndedEvent += OnAttackAnimationEnded;
            animationController.OnHideSpikesAnimationEndedEvent += OnHideSpikesAnimationEnded;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta.AttackSystemConfig _attackSystemConfig;
        private readonly SpikySlimeReferences _references;
        private readonly AggressionSystem _aggressionSystem;
        private readonly SpikySlimeAttackTrigger _attackTrigger;
        private readonly CoroutineHelper _attackRoutine;
        private readonly List<PlayerQueue> _playersQueues;
        
        private bool _attackInProgress;
        private bool _instantKill;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            float deltaTime = Time.deltaTime;

            foreach (PlayerQueue playerQueue in _playersQueues)
                playerQueue.Tick(deltaTime);
        }

        public void StartAttack()
        {
            if (_attackInProgress)
                return;

            _attackInProgress = true;

            StopSound(SpikySlimeEntity.SFXType.CalmMovement);
            PlaySound(SpikySlimeEntity.SFXType.AngryMovement);
            PlaySound(SpikySlimeEntity.SFXType.Attack);

            _spikySlimeEntity.PlayAttackAnimation();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void EnableAttackTrigger() =>
            _attackTrigger.ToggleCollider(isEnabled: true);

        private void DisableAttackTrigger() =>
            _attackTrigger.ToggleCollider(isEnabled: false);

        private void DoDamage(ulong clientID)
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;

            bool isPlayerDead = playerEntity.IsDead();

            if (isPlayerDead)
                return;

            PlaySound(SpikySlimeEntity.SFXType.Stab);

            if (_instantKill)
            {
                playerEntity.Kill(PlayerDeathReason._);
                AttachPlayerToFreeJoint(playerEntity);
            }
            else
            {
                float spikesDamage = _attackSystemConfig.SpikesDamage;
                playerEntity.TakeDamage(spikesDamage);
            }
        }

        private void AttachPlayerToFreeJoint(PlayerEntity playerEntity)
        {
            IReadOnlyList<SpringJoint> allSpringJoints = _references.GetAllSpringJoints();

            FindClosestFreeSprintJoint();

            // LOCAL METHODS: -----------------------------

            void FindFreeSprintJoint()
            {
                foreach (SpringJoint springJoint in allSpringJoints)
                {
                    bool isFree = springJoint.connectedBody == null;

                    if (!isFree)
                        continue;

                    PlayerReferences playerReferences = playerEntity.GetReferences();
                    Rigidbody spineRigidbody = playerReferences.SpineRigidbody;
                    springJoint.connectedBody = spineRigidbody;

                    break;
                }
            }

            void FindClosestFreeSprintJoint()
            {
                Vector3 playerPosition = playerEntity.transform.position;
                int iterations = allSpringJoints.Count;
                float minDistance = float.MaxValue;
                int closestIndex = 0;
                bool isFreeJointFound = false;

                for (int i = 0; i < iterations; i++)
                {
                    SpringJoint springJoint = allSpringJoints[i];
                    bool isFree = springJoint.connectedBody == null;

                    if (!isFree)
                        continue;

                    isFreeJointFound = true;

                    Vector3 jointPosition = springJoint.transform.position;
                    float distance = Vector3.Distance(a: playerPosition, b: jointPosition);

                    if (distance >= minDistance)
                        continue;

                    minDistance = distance;
                    closestIndex = i;
                }

                if (!isFreeJointFound)
                    return;

                PlayerReferences playerReferences = playerEntity.GetReferences();
                Rigidbody spineRigidbody = playerReferences.SpineRigidbody;
                allSpringJoints[closestIndex].connectedBody = spineRigidbody;
            }
        }

        private void FreeAllFromSpringJoints()
        {
            IReadOnlyList<SpringJoint> allSpringJoints = _references.GetAllSpringJoints();

            foreach (SpringJoint springJoint in allSpringJoints)
                springJoint.connectedBody = null;
        }

        private void PlaySound(SpikySlimeEntity.SFXType sfxType) =>
            _spikySlimeEntity.PlaySound(sfxType).Forget();

        private void StopSound(SpikySlimeEntity.SFXType sfxType) =>
            _spikySlimeEntity.StopSound(sfxType);

        private IEnumerator AttackCO()
        {
            _instantKill = true;

            EnableAttackTrigger();

            float instantKillDuration = _attackSystemConfig.InstantKillDuration;
            yield return new WaitForSeconds(instantKillDuration);

            _instantKill = false;

            float spikesDuration = _attackSystemConfig.SpikesDuration;
            yield return new WaitForSeconds(spikesDuration);

            _spikySlimeEntity.PlayHideSpikesAnimation();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAttack() =>
            _attackRoutine.Start();

        private void OnSpikesHidden()
        {
            _attackInProgress = false;

            _aggressionSystem.StartDecreaseTimer();

            DisableAttackTrigger();
            FreeAllFromSpringJoints();

            StopSound(SpikySlimeEntity.SFXType.AngryMovement);
            PlaySound(SpikySlimeEntity.SFXType.CalmMovement);
        }

        private void OnPlayerEnterAttackTrigger(PlayerEntity playerEntity)
        {
            ulong clientID = playerEntity.OwnerClientId;
            bool isPlayerFound = false;

            foreach (PlayerQueue playerQueue in _playersQueues)
            {
                bool isPlayerMatches = playerQueue.IsPlayerMatches(clientID);

                if (!isPlayerMatches)
                    continue;

                isPlayerFound = true;

                bool isTimeOver = playerQueue.IsTimeOver();

                if (!isTimeOver)
                    return;

                playerQueue.ResetTimer();
            }

            if (!isPlayerFound)
            {
                float damageInterval = _attackSystemConfig.SpikesDamageInterval;
                PlayerQueue playerQueue = new(clientID, damageInterval);
                _playersQueues.Add(playerQueue);
            }

            DoDamage(clientID);
        }

        private void OnAttackAnimationEnded() => OnAttack();

        private void OnHideSpikesAnimationEnded() => OnSpikesHidden();

        // INNER CLASSES: -------------------------------------------------------------------------

        private class PlayerQueue
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public PlayerQueue(ulong clientID, float timeLeft)
            {
                _clientID = clientID;
                _timeLeft = timeLeft;
                _startTime = timeLeft;
            }

            // FIELDS: --------------------------------------------------------------------------------

            private readonly ulong _clientID;
            private readonly float _startTime;

            private float _timeLeft;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public void Tick(float deltaTime) =>
                _timeLeft -= deltaTime;

            public void ResetTimer() =>
                _timeLeft = _startTime;

            public bool IsPlayerMatches(ulong clientID) =>
                _clientID == clientID;

            public bool IsTimeOver() =>
                _timeLeft <= 0.0f;
        }
    }
}