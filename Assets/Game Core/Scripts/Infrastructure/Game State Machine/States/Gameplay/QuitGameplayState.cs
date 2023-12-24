﻿using GameCore.Gameplay.Network;

namespace GameCore.Infrastructure.StateMachine
{
    public class QuitGameplayState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuitGameplayState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            TryCloseServer();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void TryCloseServer()
        {
            TheNetworkHorror network = TheNetworkHorror.Get();
            network.Disconnect();
        }

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}