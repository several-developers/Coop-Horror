using System;

namespace GameCore.Gameplay.Quests
{
    public interface IQuestsManagerDecorator
    {
        event Action OnAwaitingQuestsDataReceivedEvent; 
        event Action OnActiveQuestsDataReceivedEvent; 
        event Action<int> OnSelectQuestEvent; 
        event Func<QuestsStorage> OnGetQuestsStorageEvent;
        void AwaitingQuestsDataReceived();
        void ActiveQuestsDataReceived();
        void SelectQuest(int questID);
        QuestsStorage GetQuestsStorage();
    }
}