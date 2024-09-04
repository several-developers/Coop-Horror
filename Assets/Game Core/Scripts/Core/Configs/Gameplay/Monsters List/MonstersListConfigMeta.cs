using System.Collections.Generic;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Gameplay.MonstersList
{
    public class MonstersListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, LabelText("List")]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<MonsterReference> _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<MonsterReference> GetAllReferences() => _references;
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
    }
}