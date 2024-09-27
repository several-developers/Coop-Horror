using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.LobbiesMenu.RelayLobby
{
    public class LobbyCreationUI : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _createButton;

        [SerializeField, Required]
        private TMP_InputField _lobbyNameIF;
        
        [SerializeField, Required]
        private Toggle _isPrivateToggle;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string, bool> OnCreateClickedEvent; 

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _createButton.onClick.AddListener(OnHostClicked);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHostClicked()
        {
            string lobbyName = _lobbyNameIF.text;
            bool isPrivate = _isPrivateToggle.isOn;
            
            OnCreateClickedEvent?.Invoke(lobbyName, isPrivate);
        }
    }
}