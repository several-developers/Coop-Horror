using System;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManagerDecorator : IQuestsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnAwaitingQuestsDataReceivedEvent = delegate { };
        public event Action OnActiveQuestsDataReceivedEvent = delegate { };
        public event Action OnUpdateQuestsProgressEvent = delegate { };
        
        public event Action<int> OnSelectQuestInnerEvent = delegate { };
        public event Action<int> OnSubmitQuestItemInnerEvent = delegate { };
        public event Func<QuestsStorage> OnGetQuestsStorageInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AwaitingQuestsDataReceived() =>
            OnAwaitingQuestsDataReceivedEvent.Invoke();

        public void ActiveQuestsDataReceived() => 
            OnActiveQuestsDataReceivedEvent.Invoke();

        public void UpdateQuestsProgress() =>
            OnUpdateQuestsProgressEvent.Invoke();

        public void SelectQuest(int questID) =>
            OnSelectQuestInnerEvent.Invoke(questID);

        public void SubmitQuestItem(int itemID) =>
            OnSubmitQuestItemInnerEvent.Invoke(itemID);

        public QuestsStorage GetQuestsStorage() =>
            OnGetQuestsStorageInnerEvent?.Invoke();
    }
}