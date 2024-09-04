using System.Collections.Generic;
using CustomEditors;
using GameCore.Gameplay.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Gameplay.EntitiesList
{
    public class EntitiesListConfigMeta : EditorMeta
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesListConfigMeta() => 
            _entitiesList = new List<Entity>();

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<Entity> _entitiesList;

        [SerializeField, Required]
        private List<AssetReferenceGameObject> _entitiesReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<Entity> GetAllEntities() => _entitiesList;
        
        public IEnumerable<AssetReferenceGameObject> GetAllEntitiesReferences() => _entitiesReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
    }
}