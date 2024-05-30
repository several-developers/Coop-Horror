using GameCore.Configs.Gameplay.Enemies;

namespace GameCore.Gameplay.Entities.Monsters.Beetle.States
{
    public class AttackState : IState
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
    }
}