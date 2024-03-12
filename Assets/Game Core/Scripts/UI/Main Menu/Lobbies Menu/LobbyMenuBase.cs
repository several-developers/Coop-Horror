using System;
using System.Text.RegularExpressions;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.LobbiesMenu
{
    public abstract class LobbyMenuBase : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _joinTabButton;

        [SerializeField, Required]
        private Button _hostTabButton;
        
        [SerializeField, Required]
        private Button _closeButton;

        [SerializeField, Required]
        private CanvasGroup _joinContainerCG;
        
        [SerializeField, Required]
        private CanvasGroup _hostContainerCG;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnCloseClickedEvent; 

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        protected virtual void Awake()
        {
            DestroyOnHide();
            
            _joinTabButton.onClick.AddListener(OnJoinTabClicked);
            _hostTabButton.onClick.AddListener(OnHostTabClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected virtual void Start() => Show();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        /// <summary>
        /// Sanitize user port InputField box allowing only alphanumerics and '.'
        /// </summary>
        /// <param name="dirtyString"> string to sanitize. </param>
        /// <returns> Sanitized text string. </returns>
        public static string Sanitize(string dirtyString) =>
            Regex.Replace(input: dirtyString, pattern: "[^A-Za-z0-9.]", replacement: "");

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnJoinTabClicked()
        {
            _joinContainerCG.alpha = 1f;
            _joinContainerCG.interactable = true;
            _joinContainerCG.blocksRaycasts = true;
            
            _hostContainerCG.alpha = 0f;
            _hostContainerCG.interactable = false;
            _hostContainerCG.blocksRaycasts = false;
        }
        
        private void OnHostTabClicked()
        {
            _joinContainerCG.alpha = 0f;
            _joinContainerCG.interactable = false;
            _joinContainerCG.blocksRaycasts = false;
            
            _hostContainerCG.alpha = 1f;
            _hostContainerCG.interactable = true;
            _hostContainerCG.blocksRaycasts = true;
        }
        
        private void OnCloseClicked()
        {
            OnCloseClickedEvent?.Invoke();
            Hide();
        }
    }
}