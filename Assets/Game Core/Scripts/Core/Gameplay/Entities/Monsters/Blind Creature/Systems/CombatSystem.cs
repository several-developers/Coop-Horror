using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class CombatSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CombatSystem(BlindCreatureEntity blindCreatureEntity, AnimationController animationController)
        {
            BlindCreatureEntity.References references = blindCreatureEntity.GetReferences();
            AnimationObserver animationObserver = references.AnimationObserver;

            _blindCreatureEntity = blindCreatureEntity;
            _blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            _combatConfig = _blindCreatureAIConfig.GetCombatConfig();
            _transform = blindCreatureEntity.transform;
            _attackPoint = references.AttackPoint;
            _suspicionSystem = blindCreatureEntity.GetSuspicionSystem();
            _animationController = animationController;
            _attackCooldownRoutine = new CoroutineHelper(blindCreatureEntity);
            _collidersPull = new Collider[8];

            _attackCooldownRoutine.GetRoutineEvent += AttackCooldownCO;

            animationObserver.OnAttackEvent += OnAttack;
            animationObserver.OnAttackFinishedEvent += OnAttackFinished;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private readonly BlindCreatureAIConfigMeta.CombatConfig _combatConfig;
        private readonly Transform _transform;
        private readonly Transform _attackPoint;
        private readonly SuspicionSystem _suspicionSystem;
        private readonly AnimationController _animationController;
        private readonly CoroutineHelper _attackCooldownRoutine;
        private readonly Collider[] _collidersPull;

        private bool _isAttackOnCooldown;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TryStartAttackAnimation()
        {
            if (_blindCreatureEntity.IsDead())
                return;
            
            if (IsAttackOnCooldown())
                return;

            _isAttackOnCooldown = true;
            _animationController.PlayAttackAnimation();
            _blindCreatureEntity.PlaySound(BlindCreatureEntity.SFXType.Swing);
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

        private void TryAttack()
        {
            Vector3 position = _attackPoint.position;
            float radius = _combatConfig.TriggerRadius;
            LayerMask layerMask = _combatConfig.LayerMask;
            float damage = _combatConfig.Damage;

            int hits = Physics.OverlapSphereNonAlloc(position, radius, _collidersPull, layerMask);
            bool playSlashSound = false;

            for (int i = 0; i < hits; i++)
            {
                bool isDamageableFound = _collidersPull[i].TryGetComponent(out IDamageable damageable);

                if (!isDamageableFound)
                    continue;

                if (damageable is not PlayerEntity)
                    continue;

                bool disableAttack = _blindCreatureAIConfig.DisableAttack;

                if (disableAttack)
                    continue;
                
                playSlashSound = true;
                damageable.TakeDamage(damage, _blindCreatureEntity);
            }
            
            if (playSlashSound)
                _blindCreatureEntity.PlaySound(BlindCreatureEntity.SFXType.Slash);

            _attackCooldownRoutine.Start();
        }
        
        private IEnumerator AttackCooldownCO()
        {
            float cooldown = _combatConfig.AttackCooldown;

            yield return new WaitForSeconds(cooldown);

            _isAttackOnCooldown = false;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnAttack() => TryAttack();
        
        private void OnAttackFinished() =>
            _attackCooldownRoutine.Start();
    }
}