using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public interface IGameManagerDecorator
    {
        event Action<SceneName> OnSelectedLocationChangedEvent;
        event Action<LocationState> OnLocationStateChangedEvent; 
        void SelectedLocationChanged(SceneName locationName);
        void LocationStateChanged(LocationState locationState);
        
        event Action<SceneName> OnSelectLocationInnerEvent;
        event Func<SceneName> OnGetSelectedLocationInnerEvent;
        event Func<LocationState> OnGetLocationStateInnerEvent;
        void SelectLocation(SceneName locationName);
        SceneName GetSelectedLocation();
        LocationState GetLocationState();
    }
}