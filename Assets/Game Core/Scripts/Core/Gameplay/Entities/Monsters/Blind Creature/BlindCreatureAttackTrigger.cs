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
            bool isServer = IsServerEvent.Invoke();

            if (!isServer)
                return;
            
            OnTriggerEnterEvent.Invoke(other);
        }
    }
}