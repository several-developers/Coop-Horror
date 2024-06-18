using System;
using System.Collections;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.CombatLogics
{
    public class AttackLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackLogic(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            _transform = coroutineRunner.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTargetNotFoundEvent = delegate { };
        public event Action<PlayerEntity> OnAttackEvent = delegate { };
        public event Action OnAttackEndedEvent = delegate { };
        public event Func<PlayerEntity> GetTargetPlayerEvent = () => null;
        public event Func<float> GetAttackDistanceEvent = () => 0.5f;
        public event Func<float> GetAttackCooldownEvent = () => 1f; 

        private readonly MonoBehaviour _coroutineRunner;
        private readonly Transform _transform;
        
        private bool _isAttackOnCooldown;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryAttack()
        {
            if (_isAttackOnCooldown)
                return false;

            Attack();
            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void Attack()
        {
            PlayerEntity targetPlayer = GetTargetPlayerEvent.Invoke();

            if (targetPlayer == null)
            {
                OnTargetNotFoundEvent.Invoke();
                return;
            }

            Vector3 thisPosition = _transform.position;
            Vector3 playerPosition = targetPlayer.transform.position;
            float distance = Vector3.Distance(a: thisPosition, b: playerPosition);
            bool canAttack = distance <= GetAttackDistanceEvent.Invoke();

            if (canAttack)
            {
                OnAttackEvent.Invoke(targetPlayer);
                StartCooldownTimer();
            }

            OnAttackEndedEvent.Invoke();
        }
        
        private void StartCooldownTimer()
        {
            IEnumerator routine = CooldownTimerCO();
            _coroutineRunner.StartCoroutine(routine);
        }

        private IEnumerator CooldownTimerCO()
        {
            _isAttackOnCooldown = true;
            
            float delay = GetAttackCooldownEvent.Invoke();
            yield return new WaitForSeconds(delay);

            _isAttackOnCooldown = false;
        }
    }
}