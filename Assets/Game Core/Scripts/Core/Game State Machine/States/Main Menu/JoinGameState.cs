using GameCore.Enums.Global;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.ConnectingMenu;
using UnityEngine;

namespace GameCore.StateMachine
{
    public class JoinGameState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JoinGameState(IGameStateMachine gameStateMachine, IMenuFactory menuFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            // ConnectingMenuView connectingMenuView = CreateConnectingMenu();
            // connectingMenuView.OnQuitClickedEvent += OnQuitConnecting;
            //
            // JoinGame();


            StartClient();

        }

        public void Exit()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartClient()
        {
        }

        private static ConnectingMenuView CreateConnectingMenu() =>
            MenuStaticFactory.Create<ConnectingMenuView>();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        private void EnterPrepareGameplaySceneState() =>
            _gameStateMachine.ChangeState<PrepareGameplaySceneState>();

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

            EnterPrepareGameplaySceneState();
        }
    }
}