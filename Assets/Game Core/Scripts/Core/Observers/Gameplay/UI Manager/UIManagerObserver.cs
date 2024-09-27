using System;
using GameCore.UI.Global.MenuView;

namespace GameCore.Observers.Gameplay.UIManager
{
    public class UIManagerObserver : IUIManagerObserver
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<MenuView> OnMenuShownEvent = delegate { };
        public event Action<MenuView> OnMenuHiddenEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void MenuShown(MenuView menuView) =>
            OnMenuShownEvent.Invoke(menuView);

        public void MenuHidden(MenuView menuView) =>
            OnMenuHiddenEvent.Invoke(menuView);
    }
}