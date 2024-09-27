using System;

namespace GameCore.Gameplay.ChatManagement
{
    public interface IChatManagerDecorator
    {
        event Action<float> OnTimerTickEvent;
        event Action<string> OnChatMessageReceivedEvent;
        void SendTimerTick(float timeLeft);
        void ChatMessageReceived(string message);
        
        event Action<string> OnSendChatMessageInnerEvent;
        event Func<bool> OnCanSendMessageInnerEvent; 
        void SendChatMessage(string message);
        bool CanSendMessage();
    }
}