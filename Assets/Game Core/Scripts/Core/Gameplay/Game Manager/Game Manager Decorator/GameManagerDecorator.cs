using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManagerDecorator : IGameManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<SceneName> OnSelectedLocationChangedEvent = delegate { };
        public event Action<LocationState> OnLocationStateChangedEvent = delegate { };

        public event Action<SceneName> OnSelectLocationInnerEvent = delegate { };
        public event Func<SceneName> OnGetSelectedLocationInnerEvent;
        public event Func<LocationState> OnGetLocationStateInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SelectedLocationChanged(SceneName locationName) =>
            OnSelectedLocationChangedEvent.Invoke(locationName);
        
        public void LocationStateChanged(LocationState locationState) =>
            OnLocationStateChangedEvent.Invoke(locationState);

        public void SelectLocation(SceneName locationName) =>
            OnSelectLocationInnerEvent.Invoke(locationName);

        public SceneName GetSelectedLocation() =>
            OnGetSelectedLocationInnerEvent?.Invoke() ?? SceneName.Desert;

        public LocationState GetLocationState() =>
            OnGetLocationStateInnerEvent?.Invoke() ?? LocationState.Road;
    }
}