using CustomEditors;

namespace GameCore.Configs.Gameplay
{
    public class GameplayConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsCategory;
    }
}