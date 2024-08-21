using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class CombatSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CombatSystem(BlindCreatureEntity blindCreatureEntity, AnimationController animationController)
        {
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            _combatConfig = _blindCreatureAIConfig.GetCombatConfig();
            _transform = blindCreatureEntity.transform;
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _animationController = animationController;
            _attackCooldownRoutine = new CoroutineHelper(blindCreatureEntity);
            _collidersPull = new Collider[8];

            _attackCooldownRoutine.GetRoutineEvent += AttackCooldownCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private readonly BlindCreatureAIConfigMeta.CombatConfig _combatConfig;
        private readonly Transform _transform;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly AnimationController _animationController;
        private readonly CoroutineHelper _attackCooldownRoutine;
        private readonly Collider[] _collidersPull;

        private bool _isAttackOnCooldown;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TryAttack()
        {
            if (IsAttackOnCooldown())
                return;

            Vector3 position = _suspicionSystem.GetLastNoisePosition();
            float radius = _combatConfig.TriggerRadius;
            LayerMask layerMask = _combatConfig.LayerMask;

            int hits = Physics.OverlapSphereNonAlloc(position, radius, _collidersPull, layerMask);

            for (int i = 0; i < hits; i++)
            {
                bool isNoiseListenerFound = _collidersPull[i].TryGetComponent(out IDamageable damageable);

                if (!isNoiseListenerFound)
                    continue;

                if (damageable is not PlayerEntity playerEntity)
                    continue;

                bool disableAttack = _blindCreatureAIConfig.DisableAttack;

                if (disableAttack)
                    continue;

                playerEntity.Kill(PlayerDeathReason._);
            }

            _animationController.PlayAttackAnimation();
            _attackCooldownRoutine.Start();
        }

        public bool IsTargetAtRange()
        {
            Vector3 thisPosition = _transform.position;
            Vector3 lastNoisePosition = _suspicionSystem.GetLastNoisePosition();
            float distance = Vector3.Distance(a: thisPosition, b: lastNoisePosition);
            bool isTargetAtRange = _combatConfig.AttackDistance <= distance;
            return isTargetAtRange;
        }

        public bool IsAttackOnCooldown() => _isAttackOnCooldown;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator AttackCooldownCO()
        {
            _isAttackOnCooldown = true;
            float cooldown = _combatConfig.AttackCooldown;

            yield return new WaitForSeconds(cooldown);

            _isAttackOnCooldown = false;
        }
    }
}