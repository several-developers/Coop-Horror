using System;

namespace GameCore.Gameplay.Observers.UI
{
    public interface IUIObserver
    {
        event Action<bool> OnGameplayHUDChangedEvent;
        void ShowGameplayHUD();
        void HideGameplayHUD();
    }
}