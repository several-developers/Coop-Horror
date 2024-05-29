using CustomEditors;

namespace GameCore.Configs.Gameplay.Enemies
{
    public abstract class MonsterAIConfigMeta : EditorMeta
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.MonstersAICategory;
    }
}