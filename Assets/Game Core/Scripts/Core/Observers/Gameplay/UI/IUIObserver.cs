using System;

namespace GameCore.Observers.Gameplay.UI
{
    public interface IUIObserver
    {
        event Action<bool> OnGameplayHUDChangedEvent;
        void ShowGameplayHUD();
        void HideGameplayHUD();
    }
}