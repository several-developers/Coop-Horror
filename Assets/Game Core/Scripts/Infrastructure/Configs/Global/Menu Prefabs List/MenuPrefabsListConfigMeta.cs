using System.Collections.Generic;
using GameCore.UI.Global.MenuView;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Infrastructure.Configs.Global.MenuPrefabsList
{
    public class MenuPrefabsListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required, AssetsOnly]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private MenuView[] _menuPrefabs;

        [SerializeField]
        private List<AssetReferenceGameObject> _menuReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public MenuView[] GetMenuPrefabs() => _menuPrefabs;

        public IEnumerable<AssetReferenceGameObject> GetAllMenuReferences() => _menuReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}