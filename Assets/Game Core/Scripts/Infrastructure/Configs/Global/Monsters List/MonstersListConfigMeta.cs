using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Configs.Global.MonstersList
{
    public class MonstersListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, LabelText("List")]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<MonsterReference> _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<MonsterReference> GetAllMonstersReferences() => _references;
        
        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;

        // INNER CLASSES: -------------------------------------------------------------------------
        
        [Serializable]
        public class MonsterReference
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private MonsterType _monsterType;

            [SerializeField]
            private MonsterAIConfigMeta _monsterAIConfig;
        
            [SerializeField]
            private AssetReferenceGameObject _assetReference;

            // PROPERTIES: ----------------------------------------------------------------------------

            public MonsterType MonsterType => _monsterType;
            public MonsterAIConfigMeta MonsterAIConfig => _monsterAIConfig;
            public AssetReferenceGameObject AssetReference => _assetReference;

            private string Label => $"'Monster: {_monsterType}'";
        }
    }
}