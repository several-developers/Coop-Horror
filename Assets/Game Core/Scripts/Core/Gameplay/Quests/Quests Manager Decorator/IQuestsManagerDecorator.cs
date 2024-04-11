using System;

namespace GameCore.Gameplay.Quests
{
    public interface IQuestsManagerDecorator
    {
        event Action OnQuestsDataReceivedEvent; 
        event Func<QuestsStorage> OnGetQuestsStorageEvent;
        void QuestsDataReceived();
        QuestsStorage GetQuestsStorage();
    }
}