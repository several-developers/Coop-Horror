using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.MonstersGenerator
{
    public class MonstersGeneratorConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private MonstersSpawnAmountConfig _monstersSpawnAmountConfig;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public MonstersSpawnAmountConfig MonstersSpawnAmountConfig => _monstersSpawnAmountConfig;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}