using Sirenix.OdinInspector;

namespace GameCore.Gameplay.Entities.Other.DamageReceivers
{
    public class SimpleDamageReceiver : DamageReceiver
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TakeDamage(float damage)
        {
            DamageStaticData damageData = new(damage);
            SendDamageReceived(damageData);
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

#if UNITY_EDITOR
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugTakeDamage(float damage) => TakeDamage(damage);
#endif
    }
}