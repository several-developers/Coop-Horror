using GameCore.Gameplay.Network.ConnectionManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.MainMenu.LobbiesMenu.IPLobby
{
    public class IPLobbyMenuView : LobbyMenuBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Required]
        private IPJoiningUI _ipJoiningUI;
        
        [SerializeField, Required]
        private IPHostingUI _ipHostingUI;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _ipJoiningUI.OnJoinClickedEvent += OnJoinClicked;
            _ipHostingUI.OnHostClickedEvent += OnHostClicked;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _ipJoiningUI.OnJoinClickedEvent -= OnJoinClicked;
            _ipHostingUI.OnHostClickedEvent -= OnHostClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void JoinWithIP(string ip, string port)
        {
            int.TryParse(port, out int portNum);
            
            if (portNum <= 0)
                portNum = Constants.DefaultPort;

            ip = string.IsNullOrEmpty(ip) ? Constants.DefaultIP : ip;

            ConnectionManager connectionManager = ConnectionManager.Get();
            int randomName = Random.Range(0, 9999);
            connectionManager.StartClientIp(randomName.ToString(), ipaddress: ip, portNum);
        }
        
        private static void HostIPRequest(string ip, string port)
        {
            int.TryParse(port, out int portNum);
            
            if (portNum <= 0)
                portNum = Constants.DefaultPort;

            ip = string.IsNullOrEmpty(ip) ? Constants.DefaultIP : ip;
            
            ConnectionManager connectionManager = ConnectionManager.Get();
            int randomName = Random.Range(0, 9999);
            connectionManager.StartHostIp(randomName.ToString(), ipaddress: ip, portNum);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private static void OnJoinClicked(string ip, string port) => JoinWithIP(ip, port);

        private static void OnHostClicked(string ip, string port) => HostIPRequest(ip, port);
    }
}