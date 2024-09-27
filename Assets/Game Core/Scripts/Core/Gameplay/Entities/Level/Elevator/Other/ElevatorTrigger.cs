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

        private bool IsServer => NetworkHorror.IsTrueServer;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<IReParentable> OnTargetLeftTriggerEvent = delegate { };

        private readonly List<IReParentable> _insideTargetsList = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
                return;

            TryGetEntity(other, addToList: true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer)
                return;

            TryGetEntity(other, addToList: false);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<IReParentable> GetInsideTargetsList() => _insideTargetsList;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryGetEntity(Component other, bool addToList)
        {
            if (!other.TryGetComponent(out IReParentable parentable))
                return;

            if (addToList)
            {
                _insideTargetsList.Add(parentable);
            }
            else
            {
                _insideTargetsList.Remove(parentable);
                OnTargetLeftTriggerEvent.Invoke(parentable);
            }
        }
    }
}