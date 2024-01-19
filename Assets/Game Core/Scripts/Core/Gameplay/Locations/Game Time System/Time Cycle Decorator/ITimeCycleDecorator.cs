using System;

namespace GameCore.Gameplay.Locations.GameTime
{
    public interface ITimeCycleDecorator
    {
        event Action OnTickEvent; 
        event Action<MyDateTime> OnSyncDateTimeEvent;
        event Func<DateTime> OnGetDateTimeEvent;
        event Action OnHourPassedEvent;
        
        void Tick();
        void SyncDateTime(MyDateTime dateTime);
        void SendHourPassedEvent();
        DateTime GetDateTime();
    }
}