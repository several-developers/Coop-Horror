using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.LobbiesMenu.IPLobby
{
    public class IPHostingUI : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _hostButton;

        [SerializeField, Required]
        private TMP_InputField _ipIF;
        
        [SerializeField, Required]
        private TMP_InputField _portIF;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, string> OnHostClickedEvent; 

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _ipIF.onValueChanged.AddListener(OnIPFieldChanged);
            _portIF.onValueChanged.AddListener(OnPortFieldChanged);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SanitizeIPInputText(string text) =>
            _ipIF.text = LobbyMenuBase.Sanitize(text);

        private void SanitizePortInputText(string text) =>
            _portIF.text = LobbyMenuBase.Sanitize(text);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHostClicked()
        {
            string ip = _ipIF.text;
            string port = _portIF.text;
            
            OnHostClickedEvent?.Invoke(ip, port);
        }
        
        private void OnIPFieldChanged(string text) => SanitizeIPInputText(text);

        private void OnPortFieldChanged(string text) => SanitizePortInputText(text);
    }
}