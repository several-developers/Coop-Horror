using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Observers.Gameplay.UI
{
    public interface IUIObserver
    {
        event Action<UIEventType> OnTriggerUIEvent;
        event Action<int> OnShowRewardMenuEvent;
        void TriggerUIEvent(UIEventType eventType);
        void ShowRewardMenu(int reward);
    }
}