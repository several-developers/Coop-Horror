using System;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class PlayerTrigger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Collider _collider;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<PlayerEntity> OnPlayerEnterEvent = delegate { };

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => ChangeTriggerState(isEnabled: false);

        private void OnTriggerEnter(Collider other) => CheckForPlayerCollision(other);

        private void OnTriggerStay(Collider other) => CheckForPlayerCollision(other);

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ChangeTriggerState(bool isEnabled) =>
            _collider.enabled = isEnabled;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckForPlayerCollision(Collider other)
        {
            if (!other.TryGetComponent(out PlayerEntity playerEntity))
                return;

            OnPlayerEnterEvent.Invoke(playerEntity);
        }
    }
}