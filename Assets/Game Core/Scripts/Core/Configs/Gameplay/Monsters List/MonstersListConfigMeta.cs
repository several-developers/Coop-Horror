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
        [SerializeField, LabelText("List")]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<MonsterReference> _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

#warning ВЫНЕСТИ В ПРОВАЙДЕР С ПРОВЕРКАМИ НА ДУБЛИКАТЫ!
        public IReadOnlyList<MonsterReference> GetAllReferences() => _references;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
    }
}