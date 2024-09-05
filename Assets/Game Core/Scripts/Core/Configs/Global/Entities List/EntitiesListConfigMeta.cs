using System.Collections.Generic;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Global.EntitiesList
{
    public class EntitiesListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<AssetReferenceGameObject> _entitiesReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<AssetReferenceGameObject> GetAllReferences() => _entitiesReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsListsCategory;
    }
}