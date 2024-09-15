using System;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class SpikySlimeAttackTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Collider _collider;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<PlayerEntity> OnTriggerEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => CheckForPlayer(other);

        private void OnTriggerStay(Collider other) => CheckForPlayer(other);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ToggleCollider(bool isEnabled) =>
            _collider.enabled = isEnabled;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckForPlayer(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;
            
            OnTriggerEvent.Invoke(playerEntity);
        }
    }
}