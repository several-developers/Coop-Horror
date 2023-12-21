using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.ConnectingMenu;
using GameCore.UI.MainMenu.OnlineMenu;
using NetcodePlus;
using NetcodePlus.Demo;
using UnityEngine;

namespace GameCore.Infrastructure.StateMachine
{
    public class JoinGameState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public JoinGameState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            ConnectingMenuView connectingMenuView = CreateConnectingMenu();
            connectingMenuView.OnQuitClickedEvent += OnQuitConnecting;

            JoinGame();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static ConnectingMenuView CreateConnectingMenu() =>
            MenuFactory.Create<ConnectingMenuView>();

        private void JoinGame()
        {
            // 127.0.0.1
            string user = "Player #" + Random.Range(0, 99999);
            string character = "";
            string host = "127.0.0.1";
            
            DemoConnectData cdata = new();
            cdata.SetCharacter(character);
            TheNetwork.Get().SetConnectionExtraData(cdata);
            //SaveUser(user);
            JoinTask(user, host);
        }
        
        private async void JoinTask(string user, string host)
        {
            TheNetwork network = TheNetwork.Get();
            ushort port = NetworkData.Get().GamePort;
            
            network.Disconnect();
            //ConnectingPanel.Get().Show();

            await UniTask.Yield(); //Wait a frame after the disconnect
            
            Authenticator.Get().LoginTest(user);
            network.StartClient(host, port);
        }

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnQuitConnecting()
        {
            TheNetwork.Get()?.Disconnect();
            ClientLobby.Get()?.Disconnect();
            EnterOnlineMenuState();
        }
    }
}