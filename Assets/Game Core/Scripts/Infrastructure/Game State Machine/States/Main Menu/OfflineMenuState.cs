﻿using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.OfflineMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class OfflineMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OfflineMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateSaveSelectionMenu();
            CreateOfflineMenu();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void CreateSaveSelectionMenu() =>
            MenuFactory.Create<SaveSelectionMenuView>();

        private static void CreateOfflineMenu() =>
            MenuFactory.Create<OfflineMenuView>();
    }
}