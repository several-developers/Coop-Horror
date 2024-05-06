using System;

namespace GameCore.Observers.Gameplay.UI
{
    public interface IUIObserver
    {
        event Action<bool> OnGameplayHUDChangedEvent;
        event Action<int> OnShowRewardMenuEvent;
        void ShowGameplayHUD();
        void HideGameplayHUD();
        void ShowRewardMenu(int reward);
    }
}