using System.Collections.Generic;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Configs.Global.EntitiesList
{
    public class EntitiesListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<AssetReferenceGameObject> _dynamicEntitiesReferences;
        
        [SerializeField, Required]
        private List<AssetReferenceGameObject> _permanentEntitiesReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<AssetReferenceGameObject> GetAllDynamicReferences() => _dynamicEntitiesReferences;
        public IEnumerable<AssetReferenceGameObject> GetAllPermanentReferences() => _permanentEntitiesReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}