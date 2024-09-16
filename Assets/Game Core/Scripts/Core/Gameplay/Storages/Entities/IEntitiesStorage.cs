using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Gameplay.Storages.Entities
{
    public interface IEntitiesStorage
    {
        void AddEntity(GameObject entityGameObject);
        void Clear();
        IEnumerable<GameObject> GetAllEntities();
    }
}