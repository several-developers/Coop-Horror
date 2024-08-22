using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Systems.CombatLogics;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class AttackState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AttackState(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
            _evilClownAIConfig = evilClownEntity.GetEvilClownAIConfig();
            _attackLogic = new AttackLogic(evilClownEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
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
            _evilClownEntity.GetTargetPlayer();

        private float GetAttackDistance() =>
            _evilClownAIConfig.AttackDistance;
        
        private float GetAttackCooldown() =>
            _evilClownAIConfig.AttackCooldown;

        private void DecideStateByLocation() =>
            _evilClownEntity.DecideStateByLocation();

        private void EnterChaseState() =>
            _evilClownEntity.EnterChaseState();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTargetNotFound() => DecideStateByLocation();

        private void OnAttack(PlayerEntity targetPlayer)
        {
            bool disableAttack = _evilClownAIConfig.DisableAttack;

            if (disableAttack)
                return;
            
            targetPlayer.Kill(PlayerDeathReason._);
            _evilClownEntity.PlaySound(EvilClownEntity.SFXType.Slash);
        }

        private void OnAttackEnded() => EnterChaseState();
    }
}