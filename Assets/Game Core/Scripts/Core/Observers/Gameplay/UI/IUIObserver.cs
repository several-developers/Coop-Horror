using System;
using GameCore.Gameplay.Entities.Player;

namespace GameCore.Observers.Gameplay.UI
{
    public interface IUIObserver
    {
        event Action<bool> OnGameplayHUDChangedEvent;
        event Action<PlayerEntity> OnInitPlayerEvent; 
        void ShowGameplayHUD();
        void HideGameplayHUD();
        void InitPlayer(PlayerEntity playerEntity);
    }
}