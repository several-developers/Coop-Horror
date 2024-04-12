using System;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManagerDecorator : IQuestsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnAwaitingQuestsDataReceivedEvent = delegate { };
        public event Action OnActiveQuestsDataReceivedEvent = delegate { };
        public event Action<int> OnSelectQuestEvent = delegate {  };
        public event Func<QuestsStorage> OnGetQuestsStorageEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AwaitingQuestsDataReceived() =>
            OnAwaitingQuestsDataReceivedEvent.Invoke();

        public void ActiveQuestsDataReceived() => 
            OnActiveQuestsDataReceivedEvent.Invoke();

        public void SelectQuest(int questID) =>
            OnSelectQuestEvent.Invoke(questID);

        public QuestsStorage GetQuestsStorage() =>
            OnGetQuestsStorageEvent?.Invoke();
    }
}