using System;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManagerDecorator : IQuestsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnAwaitingQuestsDataReceivedEvent = delegate { };
        public event Action OnActiveQuestsDataReceivedEvent = delegate { };
        public event Action OnUpdateQuestsProgressEvent = delegate { };
        
        public event Action OnCalculateRewardInnerEvent = delegate { };
        public event Action<int> OnSelectQuestInnerEvent = delegate { };
        public event Action<int> OnSubmitQuestItemInnerEvent = delegate { };
        public event Action OnCompleteQuestsInnerEvent = delegate { };
        public event Action OnDecreaseQuestsDaysInnerEvent = delegate { };
        public event Action OnResetQuestsInnerEvent = delegate { };
        public event Func<QuestsStorage> OnGetQuestsStorageInnerEvent;
        public event Func<int> OnGetActiveQuestsAmountInnerEvent;
        public event Func<int, bool> OnContainsItemInQuestsInnerEvent;
        public event Func<bool> OnContainsCompletedQuestsInnerEvent;
        public event Func<bool> OnContainsExpiredQuestsInnerEvent;
        public event Func<bool> OnContainsExpiredAndUncompletedQuestsInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AwaitingQuestsDataReceived() =>
            OnAwaitingQuestsDataReceivedEvent.Invoke();

        public void ActiveQuestsDataReceived() => 
            OnActiveQuestsDataReceivedEvent.Invoke();

        public void UpdateQuestsProgress() =>
            OnUpdateQuestsProgressEvent.Invoke();


        public void CalculateReward() =>
            OnCalculateRewardInnerEvent.Invoke();

        public void SelectQuest(int questID) =>
            OnSelectQuestInnerEvent.Invoke(questID);

        public void SubmitQuestItem(int itemID) =>
            OnSubmitQuestItemInnerEvent.Invoke(itemID);

        public void CompleteQuests() =>
            OnCompleteQuestsInnerEvent.Invoke();

        public void DecreaseQuestsDays() =>
            OnDecreaseQuestsDaysInnerEvent.Invoke();

        public void ResetQuests() =>
            OnResetQuestsInnerEvent.Invoke();

        public QuestsStorage GetQuestsStorage() =>
            OnGetQuestsStorageInnerEvent?.Invoke();

        public int GetActiveQuestsAmount() =>
            OnGetActiveQuestsAmountInnerEvent?.Invoke() ?? 0;

        public bool ContainsItemInQuests(int itemID) =>
            OnContainsItemInQuestsInnerEvent?.Invoke(itemID) ?? false;

        public bool ContainsCompletedQuests() =>
            OnContainsCompletedQuestsInnerEvent?.Invoke() ?? false;

        public bool ContainsExpiredQuests() =>
            OnContainsExpiredQuestsInnerEvent?.Invoke() ?? false;

        public bool ContainsExpiredAndUncompletedQuests() =>
            OnContainsExpiredAndUncompletedQuestsInnerEvent?.Invoke() ?? false;
    }
}