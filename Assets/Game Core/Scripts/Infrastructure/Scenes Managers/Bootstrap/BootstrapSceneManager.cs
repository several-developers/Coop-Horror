using GameCore.Enums;
using GameCore.Infrastructure.Services.Global;
using GameCore.Infrastructure.Services.Global.Rewards;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.ScenesManagers.BootstrapScene
{
    public class BootstrapSceneManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService scenesLoaderService) =>
            _scenesLoaderService = scenesLoaderService;

        // FIELDS: --------------------------------------------------------------------------------

        private IScenesLoaderService _scenesLoaderService;
        private IRewardsService _rewardsService;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            // Load some SDK or assets

            LoadGameScene();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadGameScene() =>
            _scenesLoaderService.LoadScene(SceneName.Gameplay);
    }
}