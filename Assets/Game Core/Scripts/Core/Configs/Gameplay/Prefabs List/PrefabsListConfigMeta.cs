using CustomEditors;
using GameCore.Gameplay.Levels.Elevator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.PrefabsList
{
    public class PrefabsListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ElevatorsManager _elevatorsManager;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}