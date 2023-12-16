using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class LevelMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private LevelManager _levelPrefab;

        // PROPERTIES: ----------------------------------------------------------------------------

        public LevelManager LevelPrefab => _levelPrefab;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.LevelsCategory;
    }
}