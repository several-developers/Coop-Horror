using System;

namespace GameCore.Gameplay.Observers.UI
{
    public class UIObserver : IUIObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<bool> OnGameplayHUDChangedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ShowGameplayHUD() =>
            OnGameplayHUDChangedEvent?.Invoke(true);

        public void HideGameplayHUD() =>
            OnGameplayHUDChangedEvent?.Invoke(false);
    }
}