using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.PubSub;
using GameCore.Gameplay.PubSub.Messages;

namespace GameCore.Observers.Gameplay.UI
{
    public class UIObserver : IUIObserver, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public UIObserver(ISubscriber<UIEventMessage> uiEventMessageSubscriber)
        {
            _uiEventMessageSubscriber = uiEventMessageSubscriber;

            _uiEventMessageSubscriber.Subscribe(OnUIEventMessageReceived);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<UIEventType> OnTriggerUIEvent = delegate { };
        public event Action<int> OnShowRewardMenuEvent = delegate { };
        
        private readonly ISubscriber<UIEventMessage> _uiEventMessageSubscriber;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _uiEventMessageSubscriber.Unsubscribe(OnUIEventMessageReceived);

        public void TriggerUIEvent(UIEventType eventType) =>
            OnTriggerUIEvent.Invoke(eventType);

        public void ShowRewardMenu(int reward) =>
            OnShowRewardMenuEvent.Invoke(reward);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnUIEventMessageReceived(UIEventMessage uiEventMessage)
        {
            UIEventType eventType = uiEventMessage.UIEventType;
            TriggerUIEvent(eventType);
        }
    }
}