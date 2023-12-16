using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Other.DamageReceivers
{
    public abstract class DamageReceiver : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<DamageStaticData> OnDamageReceivedEvent;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void SendDamageReceived(DamageStaticData damageData) =>
            OnDamageReceivedEvent?.Invoke(damageData);
    }
}