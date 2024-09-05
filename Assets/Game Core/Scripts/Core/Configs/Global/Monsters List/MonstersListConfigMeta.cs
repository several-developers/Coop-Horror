using System.Collections.Generic;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Global.MonstersList
{
    public class MonstersListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, LabelText("List")]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<MonsterReference> _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<MonsterReference> GetAllReferences() => _references;
        
        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}