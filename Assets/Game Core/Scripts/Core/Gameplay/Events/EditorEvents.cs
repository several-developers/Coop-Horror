using UnityEngine.Events;

#if UNITY_EDITOR
namespace GameCore.Gameplay.Events
{
    public static class EditorEvents
    {
        // RECEIVERS: -----------------------------------------------------------------------------
        
        public static readonly UnityEvent OnDataChangedEvent = new();

        // SENDERS: -------------------------------------------------------------------------------

        public static void SendDataChanged() => OnDataChangedEvent?.Invoke();

        public static bool IsDataEventEmpty() =>
            OnDataChangedEvent.GetPersistentEventCount() == 0;
    }
}
#endif