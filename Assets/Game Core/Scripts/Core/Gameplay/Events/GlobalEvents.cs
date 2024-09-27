using UnityEngine.Events;

namespace GameCore.Gameplay.Events
{
    public static class GlobalEvents
    {
        // RECEIVERS: -----------------------------------------------------------------------------
        
        public static readonly UnityEvent OnCurrencyChanged = new();

        // SENDERS: -------------------------------------------------------------------------------

        public static void SendCurrencyChanged() => OnCurrencyChanged?.Invoke();
    }
}