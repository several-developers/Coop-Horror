using System;
using GameCore.Gameplay.Entities.Player;

namespace GameCore.Observers.Gameplay.UI
{
    public interface IUIObserver
    {
        event Action<bool> OnGameplayHUDChangedEvent;
        event Action<PlayerEntity> OnInitPlayerEvent;
        event Action<int> OnShowRewardMenuEvent;
        void ShowGameplayHUD();
        void HideGameplayHUD();
        void InitPlayer(PlayerEntity playerEntity);
        void ShowRewardMenu(int reward);
    }
}