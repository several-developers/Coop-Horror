using System;
using System.Collections.Generic;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    public class ElevatorTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawTrigger;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawTrigger => _drawTrigger;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Entity> OnEntityLeftEvent = delegate { }; 

        private readonly List<Entity> _insideEntitiesList = new();
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => TryGetEntity(other, addToList: true);

        private void OnTriggerExit(Collider other) => TryGetEntity(other, addToList: false);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<Entity> GetInsideEntitiesList() => _insideEntitiesList;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryGetEntity(Component other, bool addToList)
        {
            if (!NetworkHorror.IsTrueServer)
                return;
            
            if (!other.TryGetComponent(out Entity entity))
                return;

            if (addToList)
            {
                _insideEntitiesList.Add(entity);
            }
            else
            {
                _insideEntitiesList.Remove(entity);
                OnEntityLeftEvent.Invoke(entity);
            }
        }
    }
}