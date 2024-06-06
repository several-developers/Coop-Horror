using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class AttackState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackState(BeetleEntity beetleEntity, BeetleAIConfigMeta beetleAIConfig)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        
        private bool _canAttack = true;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            if (_canAttack)
                TryAttack();
            else
                EnterChaseState();
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryAttack()
        {
            PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();

            if (targetPlayer == null)
            {
                DecideStateByLocation();
                return;
            }

            Vector3 beetlePosition = _beetleEntity.transform.position;
            Vector3 playerPosition = targetPlayer.transform.position;
            float distance = Vector3.Distance(a: beetlePosition, b: playerPosition);
            bool canAttack = distance <= _beetleAIConfig.AttackDistance;

            if (canAttack)
            {
                DealDamage();
                StartCooldownTimer();
            }

            EnterChaseState();
        }

        private void DealDamage()
        {
            float damage = _beetleAIConfig.Damage;
            PlayerEntity targetPlayer = _beetleEntity.GetTargetPlayer();
            targetPlayer.TakeDamage(damage);
        }
        
        private void StartCooldownTimer()
        {
            IEnumerator routine = CooldownTimerCO();
            _beetleEntity.StartCoroutine(routine);
        }

        private IEnumerator CooldownTimerCO()
        {
            _canAttack = false;
            
            float delay = _beetleAIConfig.AttackCooldown;
            yield return new WaitForSeconds(delay);

            _canAttack = true;
        }
        
        private void DecideStateByLocation() =>
            _beetleEntity.DecideStateByLocation();

        private void EnterChaseState() =>
            _beetleEntity.EnterChaseState();
    }
}