using CustomEditors;

namespace GameCore.Configs.Gameplay.Entities
{
    public abstract class EntityConfigMeta : EditorMeta
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() => 
            EditorConstants.EntitiesConfigsCategory;
    }
}