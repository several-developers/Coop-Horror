using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class AttackState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackState(EvilClownEntity evilClownEntity, EvilClownAIConfigMeta evilClownAIConfig)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownAIConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        
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
            PlayerEntity targetPlayer = _evilClownEntity.GetTargetPlayer();

            if (targetPlayer == null)
            {
                DecideStateByLocation();
                return;
            }

            Vector3 beetlePosition = _evilClownEntity.transform.position;
            Vector3 playerPosition = targetPlayer.transform.position;
            float distance = Vector3.Distance(a: beetlePosition, b: playerPosition);
            bool canAttack = distance <= _evilClownAIConfig.AttackDistance;

            if (canAttack)
            {
                KillTargetPlayer();
                StartCooldownTimer();
            }

            EnterChaseState();
        }

        private void KillTargetPlayer()
        {
            PlayerEntity targetPlayer = _evilClownEntity.GetTargetPlayer();
            targetPlayer.Kill(PlayerDeathReason._);
        }
        
        private void StartCooldownTimer()
        {
            IEnumerator routine = CooldownTimerCO();
            _evilClownEntity.StartCoroutine(routine);
        }

        private IEnumerator CooldownTimerCO()
        {
            _canAttack = false;
            
            float delay = _evilClownAIConfig.AttackCooldown;
            yield return new WaitForSeconds(delay);

            _canAttack = true;
        }
        
        private void DecideStateByLocation() =>
            _evilClownEntity.DecideStateByLocation();

        private void EnterChaseState() =>
            _evilClownEntity.EnterChaseState();
    }
}