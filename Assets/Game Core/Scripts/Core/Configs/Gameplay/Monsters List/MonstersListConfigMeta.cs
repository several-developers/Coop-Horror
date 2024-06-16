using System.Collections.Generic;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.MonstersList
{
    public class MonstersListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<MonsterReference> _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<MonsterReference> GetAllReferences() => _references;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}