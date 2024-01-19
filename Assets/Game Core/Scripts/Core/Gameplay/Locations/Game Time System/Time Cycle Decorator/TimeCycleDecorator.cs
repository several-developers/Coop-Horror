using System;

namespace GameCore.Gameplay.Locations.GameTime
{
    public class TimeCycleDecorator : ITimeCycleDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTickEvent;
        public event Action<MyDateTime> OnSyncDateTimeEvent;
        public event Func<DateTime> OnGetDateTimeEvent;
        public event Action OnHourPassedEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Tick() =>
            OnTickEvent?.Invoke();

        public void SyncDateTime(MyDateTime dateTime) =>
            OnSyncDateTimeEvent?.Invoke(dateTime);

        public void SendHourPassedEvent() =>
            OnHourPassedEvent?.Invoke();

        public DateTime GetDateTime()
        {
            if (OnGetDateTimeEvent == null)
                return new DateTime();
            
            return OnGetDateTimeEvent();
        }
    }
}