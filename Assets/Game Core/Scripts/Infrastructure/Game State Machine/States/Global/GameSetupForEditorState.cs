using GameCore.Configs.Game;
using GameCore.Enums;
using GameCore.Gameplay;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Services.Global;
using UnityEngine.SceneManagement;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameSetupForEditorState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameSetupForEditorState(IGameStateMachine gameStateMachine, IConfigsProvider configsProvider,
            IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;
            _gameConfig = configsProvider.GetGameConfig();

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;
        private readonly GameConfigMeta _gameConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            bool useStartScene = _gameConfig.UseStartScene;

            if (useStartScene)
            {
                CheckGameConfig();
                return;
            }

            if (!IsCurrentSceneMainMenu())
                return;

            EnterPrepareMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGameConfig()
        {
            SceneName startScene = _gameConfig.StartScene;
            bool forceLoadBootstrapScene = _gameConfig.ForceLoadBootstrapScene;
            bool loadOnlyBootstrapScene = forceLoadBootstrapScene && startScene == SceneName.Bootstrap;

            if (forceLoadBootstrapScene)
                _scenesLoaderService.LoadScene(SceneName.Bootstrap, OnSceneLoaded);
            else
                _scenesLoaderService.LoadScene(startScene);

            void OnSceneLoaded()
            {
                if (loadOnlyBootstrapScene)
                    return;

                _scenesLoaderService.LoadScene(startScene);
            }
        }

        private bool IsCurrentSceneMainMenu()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isSceneMainMenu = string.Equals(sceneName, SceneName.MainMenu.ToString());
            return isSceneMainMenu;
        }

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();
    }
}