using GameCore.Gameplay.Factories;
using GameCore.UI.Gameplay.DeveloperMenu;
using GameCore.UI.Global.Buttons;
using Zenject;

namespace GameCore.UI.Gameplay.HUD
{
    public class OpenDeveloperMenuButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(DiContainer diContainer) =>
            _diContainer = diContainer;

        // FIELDS: --------------------------------------------------------------------------------

        private DiContainer _diContainer;
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic() =>
            MenuFactory.Create<DeveloperMenuView>(_diContainer);
    }
}