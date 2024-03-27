using System;
using GameCore.Gameplay.Entities.Player;

namespace GameCore.Observers.Gameplay.UI
{
    public class UIObserver : IUIObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<bool> OnGameplayHUDChangedEvent;
        public event Action<PlayerEntity> OnInitPlayerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void ShowGameplayHUD() =>
            OnGameplayHUDChangedEvent?.Invoke(true);

        public void HideGameplayHUD() =>
            OnGameplayHUDChangedEvent?.Invoke(false);

        public void InitPlayer(PlayerEntity playerEntity) =>
            OnInitPlayerEvent?.Invoke(playerEntity);
    }
}