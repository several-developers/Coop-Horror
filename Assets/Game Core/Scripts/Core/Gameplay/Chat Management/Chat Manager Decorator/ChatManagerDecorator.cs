using System;

namespace GameCore.Gameplay.ChatManagement
{
    public class ChatManagerDecorator : IChatManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<float> OnTimerTickEvent = delegate { };
        public event Action<string> OnChatMessageReceivedEvent = delegate { };
        
        public event Action<string> OnSendChatMessageInnerEvent = delegate { };
        public event Func<bool> OnCanSendMessageInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SendTimerTick(float timeLeft) =>
            OnTimerTickEvent.Invoke(timeLeft);

        public void ChatMessageReceived(string message) =>
            OnChatMessageReceivedEvent.Invoke(message);

        public void SendChatMessage(string message) =>
            OnSendChatMessageInnerEvent.Invoke(message);

        public bool CanSendMessage() =>
            OnCanSendMessageInnerEvent?.Invoke() ?? false;
    }
}