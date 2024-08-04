using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.EntitiesSystems.CombatLogics;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class AttackState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackState(BeetleEntity beetleEntity)
        {
            _beetleEntity = beetleEntity;
            _beetleAIConfig = beetleEntity.GetAIConfig();
            _attackLogic = new AttackLogic(beetleEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly BeetleEntity _beetleEntity;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly AttackLogic _attackLogic;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _attackLogic.OnTargetNotFoundEvent += OnTargetNotFound;
            _attackLogic.OnAttackEvent += OnAttack;
            _attackLogic.OnAttackEndedEvent += OnAttackEnded;
            _attackLogic.GetTargetPlayerEvent += GetTargetPlayer;
            _attackLogic.GetAttackDistanceEvent += GetAttackDistance;
            _attackLogic.GetAttackCooldownEvent += GetAttackCooldown;
            
            if (!_attackLogic.TryAttack())
                EnterChaseState();
        }

        public void Exit()
        {
            _attackLogic.OnTargetNotFoundEvent -= OnTargetNotFound;
            _attackLogic.OnAttackEvent -= OnAttack;
            _attackLogic.OnAttackEndedEvent -= OnAttackEnded;
            _attackLogic.GetTargetPlayerEvent -= GetTargetPlayer;
            _attackLogic.GetAttackDistanceEvent -= GetAttackDistance;
            _attackLogic.GetAttackCooldownEvent -= GetAttackCooldown;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private PlayerEntity GetTargetPlayer() =>
            _beetleEntity.GetTargetPlayer();

        private float GetAttackDistance() =>
            _beetleAIConfig.AttackDistance;

        private float GetAttackCooldown() =>
            _beetleAIConfig.AttackCooldown;

        private void DecideStateByLocation() =>
            _beetleEntity.DecideStateByLocation();

        private void EnterChaseState() =>
            _beetleEntity.EnterChaseState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => DecideStateByLocation();

        private void OnAttack(PlayerEntity targetPlayer)
        {
            float damage = _beetleAIConfig.Damage;
            targetPlayer.TakeDamage(damage);
        }

        private void OnAttackEnded() => EnterChaseState();
    }
}