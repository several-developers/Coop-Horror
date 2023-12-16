using GameCore.Enums;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.Global.Buttons;
using Zenject;

namespace GameCore.UI.Gameplay.HUD
{
    public class RestartSceneButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService scenesLoaderService) =>
            _scenesLoaderService = scenesLoaderService;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IScenesLoaderService _scenesLoaderService;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void ClickLogic()
        {
            _scenesLoaderService.LoadScene(SceneName.Gameplay);
            gameObject.SetActive(false);
        }
    }
}