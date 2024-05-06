using System;

namespace GameCore.Observers.Gameplay.UI
{
    public class UIObserver : IUIObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<bool> OnGameplayHUDChangedEvent = delegate { };
        public event Action<int> OnShowRewardMenuEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ShowGameplayHUD() =>
            OnGameplayHUDChangedEvent.Invoke(true);

        public void HideGameplayHUD() =>
            OnGameplayHUDChangedEvent.Invoke(false);

        public void ShowRewardMenu(int reward) =>
            OnShowRewardMenuEvent.Invoke(reward);
    }
}