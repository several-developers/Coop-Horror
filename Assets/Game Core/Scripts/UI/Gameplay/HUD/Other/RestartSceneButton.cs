using GameCore.UI.Global.Buttons;
using Zenject;

namespace GameCore.UI.Gameplay.HUD
{
    public class RestartSceneButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct()
        {
        }

        // FIELDS: --------------------------------------------------------------------------------
        

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic()
        {
            gameObject.SetActive(false);
        }
    }
}