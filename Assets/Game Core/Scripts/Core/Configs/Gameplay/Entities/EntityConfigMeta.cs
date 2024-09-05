using GameCore.Infrastructure.Configs;

namespace GameCore.Configs.Gameplay.Entities
{
    public abstract class EntityConfigMeta : ConfigMeta
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() => 
            EditorConstants.EntitiesConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}