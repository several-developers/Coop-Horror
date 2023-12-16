using GameCore.Gameplay.Events;
using GameCore.Infrastructure.Services.Global.Data;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.DeveloperMenu.CheatButtons
{
    public class ResetGoldButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IPlayerDataService playerDataService) =>
            _playerDataService = playerDataService;
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _button;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IPlayerDataService _playerDataService;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            GlobalEvents.OnCurrencyChanged.AddListener(OnCurrencyChanged);
        }
        
        private void Start() => CheckButtonState();

        private void OnDestroy() =>
            GlobalEvents.OnCurrencyChanged.RemoveListener(OnCurrencyChanged);

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic()
        {
            ResetGold();
            SendCurrencyChanged();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckButtonState()
        {
            long playerGold = _playerDataService.GetGold();
            bool isEnabled = playerGold > 0;

            SetButtonState(isEnabled);
        }

        private void SetButtonState(bool isInteractable) =>
            _button.interactable = isInteractable;

        private void ResetGold() =>
            _playerDataService.RemoveGold(long.MaxValue);

        private static void SendCurrencyChanged() =>
            GlobalEvents.SendCurrencyChanged();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnCurrencyChanged() => CheckButtonState();
    }
}