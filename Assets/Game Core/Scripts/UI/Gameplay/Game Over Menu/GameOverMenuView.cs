using System.Collections;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Gameplay.GameOverMenu
{
    public class GameOverMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _hideDelay = 3f;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            StartCoroutine(routine: HideTimerCO());
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator HideTimerCO()
        {
            yield return new WaitForSeconds(_hideDelay);

            Hide();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugShow() => Show();
    }
}