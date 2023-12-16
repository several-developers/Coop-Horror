using GameCore.Gameplay.Events;
using GameCore.Infrastructure.Services.Global.Rewards;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.DeveloperMenu.CheatButtons
{
    public class GiveGoldButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRewardsService rewardsService) =>
            _rewardsService = rewardsService;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private long _amount;

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private IRewardsService _rewardsService;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => UpdateText();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic()
        {
            _rewardsService.AddGold(_amount);
            GlobalEvents.SendCurrencyChanged();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateText() =>
            _textTMP.text = $"Gold <color=#FF2135>+{_amount}</color>";
    }
}