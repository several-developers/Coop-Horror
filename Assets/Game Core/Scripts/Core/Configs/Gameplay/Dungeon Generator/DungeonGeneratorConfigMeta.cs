using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.DungeonGenerator
{
    public class DungeonGeneratorConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        

        // PROPERTIES: ----------------------------------------------------------------------------
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}