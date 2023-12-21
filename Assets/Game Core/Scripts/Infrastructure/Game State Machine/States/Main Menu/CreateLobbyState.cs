using Cysharp.Threading.Tasks;
using GameCore.Enums;
using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.MainMenu.CreateLobbyMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;
using GameCore.Utilities;
using NetcodePlus;
using NetcodePlus.Demo;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateLobbyState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateLobbyState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;

        private CreateLobbyMenuView _createLobbyMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateCreateLobbyMenu();
            CreateSaveSelectionMenuView();

            _createLobbyMenuView.OnStartGameClickedEvent += OnStartGameClicked;
        }

        public void Exit() =>
            _createLobbyMenuView.OnStartGameClickedEvent -= OnStartGameClicked;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateCreateLobbyMenu() =>
            _createLobbyMenuView = MenuFactory.Create<CreateLobbyMenuView>();

        private void CreateGame()
        {
            //GameModeData gameModeData = GameModeData.Get(GameMode.Simple);
            string character = "";
            string username = "Player #" + Random.Range(0, 99999);
            string scene = SceneName.Gameplay.ToString();
            
            DemoConnectData cdata = new(GameMode.Simple);
            cdata.SetCharacter(character);
            TheNetwork.Get().SetConnectionExtraData(cdata);
            //SaveUser(user);
            ConnectionTask(username, scene);
        }
        
        private async void ConnectionTask(string username, string scene)
        {
            TheNetwork network = TheNetwork.Get();
            ushort port = NetworkData.Get().GamePort;
            
            network.Disconnect();
            
            // Show black fade
            
            await UniTask.Yield(); //Wait a frame after the disconnect
            
            Authenticator.Get().LoginTest(username);
            network.StartHost(port);
            network.LoadScene(scene, loadOfflineCallback: LoadGameplayScene); // HERE MAYBE PROBLEM
        }

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadSceneNetwork(SceneName.Gameplay);

        private static void CreateSaveSelectionMenuView() =>
           MenuFactory.Create<SaveSelectionMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartGameClicked() => CreateGame();
    }
}