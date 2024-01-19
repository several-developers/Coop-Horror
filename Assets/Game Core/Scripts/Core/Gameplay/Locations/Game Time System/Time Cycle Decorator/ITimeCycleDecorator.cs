using System;

namespace GameCore.Gameplay.Locations.GameTime
{
    public interface ITimeCycleDecorator
    {
        event Action OnUpdateLogicEvent; 
        event Action<MyDateTime> OnSyncDateTimeEvent;
        event Func<DateTime> OnGetDateTimeEvent;
        event Action OnHourPassedEvent;
        
        void UpdateLogic();
        void SyncDateTime(MyDateTime dateTime);
        void SendHourPassedEvent();
        DateTime GetDateTime();
    }
}