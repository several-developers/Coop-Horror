using System;
using System.Collections.Generic;
using GameCore.Gameplay.InputManagement;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;

namespace GameCore.Gameplay.UIManagement
{
    public class UIManager : IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public UIManager(IUIManagerObserver uiManagerObserver, IConfigsProvider configsProvider)
        {
            _uiManagerObserver = uiManagerObserver;
            _inputReader = configsProvider.GetInputReader();
            _menus = new Stack<MenuView>();

            _uiManagerObserver.OnMenuShownEvent += OnMenuShown;
            _uiManagerObserver.OnMenuHiddenEvent += OnMenuHidden;
            
            _inputReader.OnResumeEvent += OnResume;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IUIManagerObserver _uiManagerObserver;
        private readonly InputReader _inputReader;
        private readonly Stack<MenuView> _menus;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            _uiManagerObserver.OnMenuShownEvent -= OnMenuShown;
            _uiManagerObserver.OnMenuHiddenEvent -= OnMenuHidden;
            
            _inputReader.OnResumeEvent -= OnResume;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void CheckInputState()
        {
            int menusAmount = _menus.Count;

            if (menusAmount > 0)
                EnableUIInput();
            else
                EnableGameplayInput();
        }
        
        private void EnableGameplayInput()
        {
            LockCursor();
            _inputReader.EnableGameplayInput();
        }

        private void EnableUIInput()
        {
            UnlockCursor();
            _inputReader.EnableUIInput();
        }

        private static void LockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: true);

        private static void UnlockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: false);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnResume()
        {
            bool containsAnyMenu = _menus.Count > 0;

            if (containsAnyMenu)
            {
                MenuView menuView = _menus.Peek();
                menuView.Hide();
            }
            else EnableGameplayInput();
        }

        private void OnMenuShown(MenuView menuView)
        {
            _menus.Push(menuView);
            CheckInputState();
        }

        private void OnMenuHidden(MenuView menuView)
        {
            _menus.Pop();
            CheckInputState();
        }
    }
}