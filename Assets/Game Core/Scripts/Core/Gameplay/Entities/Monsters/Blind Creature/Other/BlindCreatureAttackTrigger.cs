using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class BlindCreatureAttackTrigger : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Collider> OnTriggerEnterEvent = delegate { };
        public event Func<bool> IsServerEvent = () => false;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer())
                return;
            
            OnTriggerEnterEvent.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsServer())
                return;
            
            OnTriggerEnterEvent.Invoke(other);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsServer() =>
            IsServerEvent.Invoke();
    }
}