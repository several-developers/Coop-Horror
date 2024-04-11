using System;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManagerDecorator : IQuestsManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnQuestsDataReceivedEvent = delegate { };
        public event Func<QuestsStorage> OnGetQuestsStorageEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void QuestsDataReceived() =>
            OnQuestsDataReceivedEvent.Invoke();

        public QuestsStorage GetQuestsStorage() =>
            OnGetQuestsStorageEvent?.Invoke();
    }
}