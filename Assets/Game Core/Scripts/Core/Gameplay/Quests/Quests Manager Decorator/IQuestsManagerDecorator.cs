using System;

namespace GameCore.Gameplay.Quests
{
    public interface IQuestsManagerDecorator
    {
        event Action OnAwaitingQuestsDataReceivedEvent; 
        event Action OnActiveQuestsDataReceivedEvent; 
        event Action OnUpdateQuestsProgressEvent;
        void AwaitingQuestsDataReceived();
        void ActiveQuestsDataReceived();
        void UpdateQuestsProgress();
        
        event Action<int> OnSelectQuestInnerEvent;
        event Action<int> OnSubmitQuestItemInnerEvent;
        event Func<QuestsStorage> OnGetQuestsStorageInnerEvent;
        void SelectQuest(int questID);
        void SubmitQuestItem(int itemID);
        QuestsStorage GetQuestsStorage();
    }
}