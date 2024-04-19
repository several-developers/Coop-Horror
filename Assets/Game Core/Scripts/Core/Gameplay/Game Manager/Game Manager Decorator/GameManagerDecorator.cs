using System;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManagerDecorator : IGameManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<SceneName> OnSelectedLocationChangedEvent = delegate { };
        
        public event Action<SceneName> OnSelectLocationInnerEvent = delegate { };
        public event Func<SceneName> OnGetSelectedLocationInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SelectedLocationChanged(SceneName locationName) =>
            OnSelectedLocationChangedEvent.Invoke(locationName);

        public void SelectLocation(SceneName locationName) =>
            OnSelectLocationInnerEvent.Invoke(locationName);

        public SceneName GetSelectedLocation() =>
            OnGetSelectedLocationInnerEvent?.Invoke() ?? SceneName.Desert;
    }
}