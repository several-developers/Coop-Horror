using System.Collections.Generic;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Global.EntitiesList
{
    public class EntitiesListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<AssetReferenceGameObject> _entitiesReferences;
        
        [SerializeField, Required]
        private List<AssetReferenceGameObject> _entitiesNetworkReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<AssetReferenceGameObject> GetAllReferences() => _entitiesReferences;
        public IEnumerable<AssetReferenceGameObject> GetAllNetworkReferences() => _entitiesNetworkReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}