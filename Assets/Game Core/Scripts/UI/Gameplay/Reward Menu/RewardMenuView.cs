using System.Collections;
using GameCore.Observers.Gameplay.UI;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.RewardMenu
{
    public class RewardMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IUIObserver uiObserver) =>
            _uiObserver = uiObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _hideDelay = 3f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _rewardTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private IUIObserver _uiObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _uiObserver.OnShowRewardMenuEvent += OnShowRewardMenu;

        private void OnDestroy() =>
            _uiObserver.OnShowRewardMenuEvent -= OnShowRewardMenu;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowMenu(int reward)
        {
            _rewardTMP.text = $"Reward:\n{reward}";
            Show();

            StartCoroutine(routine: HideTimerCO());
        }

        private IEnumerator HideTimerCO()
        {
            yield return new WaitForSeconds(_hideDelay);
            
            Hide();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnShowRewardMenu(int reward) => ShowMenu(reward);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugShowMenu(int reward) => ShowMenu(reward);
    }
}