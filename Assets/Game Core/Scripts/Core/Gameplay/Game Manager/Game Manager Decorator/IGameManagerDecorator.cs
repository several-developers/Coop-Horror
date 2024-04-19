using System;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public interface IGameManagerDecorator
    {
        event Action<SceneName> OnSelectedLocationChangedEvent;
        void SelectedLocationChanged(SceneName locationName);
        
        event Action<SceneName> OnSelectLocationInnerEvent;
        event Func<SceneName> OnGetSelectedLocationInnerEvent;
        void SelectLocation(SceneName locationName);
        SceneName GetSelectedLocation();
    }
}