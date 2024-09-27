using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Gameplay.Storages.Entities
{
    public class EntitiesStorage : IEntitiesStorage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesStorage() =>
            _allEntities = new LinkedList<GameObject>();

        // FIELDS: --------------------------------------------------------------------------------

        private readonly LinkedList<GameObject> _allEntities;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AddEntity(GameObject entityGameObject) =>
            _allEntities.AddLast(entityGameObject);

        public void Clear() =>
            _allEntities.Clear();
        
        public IEnumerable<GameObject> GetAllEntities() => _allEntities;
    }
}