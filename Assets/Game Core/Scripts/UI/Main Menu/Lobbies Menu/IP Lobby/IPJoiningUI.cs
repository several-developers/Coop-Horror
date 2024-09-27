using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.LobbiesMenu.IPLobby
{
    public class IPJoiningUI : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _joinButton;

        [SerializeField, Required]
        private TMP_InputField _ipIF;
        
        [SerializeField, Required]
        private TMP_InputField _portIF;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, string> OnJoinClickedEvent; 

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _joinButton.onClick.AddListener(OnJoinClicked);
            _ipIF.onValueChanged.AddListener(OnIPFieldChanged);
            _portIF.onValueChanged.AddListener(OnPortFieldChanged);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SanitizeIPInputText(string text) =>
            _ipIF.text = LobbyMenuBase.Sanitize(text);

        private void SanitizePortInputText(string text) =>
            _portIF.text = LobbyMenuBase.Sanitize(text);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnJoinClicked()
        {
            string ip = _ipIF.text;
            string port = _portIF.text;
            
            OnJoinClickedEvent?.Invoke(ip, port);
        }
        
        private void OnIPFieldChanged(string text) => SanitizeIPInputText(text);

        private void OnPortFieldChanged(string text) => SanitizePortInputText(text);
    }
}