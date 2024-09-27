using System;
using GameCore.UI.Global.MenuView;

namespace GameCore.Observers.Gameplay.UIManager
{
    public interface IUIManagerObserver
    {
        event Action<MenuView> OnMenuShownEvent;
        event Action<MenuView> OnMenuHiddenEvent;
        void MenuShown(MenuView menuView);
        void MenuHidden(MenuView menuView);
    }
}