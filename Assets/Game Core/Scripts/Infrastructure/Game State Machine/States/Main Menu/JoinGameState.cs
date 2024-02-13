using GameCore.Enums.Global;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Network;
using GameCore.UI.MainMenu.ConnectingMenu;
using UnityEngine;

namespace GameCore.Infrastructure.StateMachine
{
    public class JoinGameState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JoinGameState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private TheNetworkHorror _network;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            // ConnectingMenuView connectingMenuView = CreateConnectingMenu();
            // connectingMenuView.OnQuitClickedEvent += OnQuitConnecting;
            //
            // JoinGame();

            _network = TheNetworkHorror.Get();

            StartClient();

            _network.OnAfterChangeSceneEvent += OnAfterChangeScene;
        }

        public void Exit() =>
            _network.OnAfterChangeSceneEvent -= OnAfterChangeScene;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartClient() =>
            _network.StartClient();

        private static ConnectingMenuView CreateConnectingMenu() =>
            MenuFactory.Create<ConnectingMenuView>();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        private void EnterGameplayState() =>
            _gameStateMachine.ChangeState<GameplayState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnQuitConnecting() => EnterOnlineMenuState();

        private void OnAfterChangeScene(string sceneName)
        {
            bool isGameplayScene = string.Equals(sceneName, SceneName.Gameplay.ToString());

            if (!isGameplayScene)
            {
                Debug.Log($"Loaded scene ({sceneName}), wrong!!!");
                return;
            }

            EnterGameplayState();
        }
    }
}