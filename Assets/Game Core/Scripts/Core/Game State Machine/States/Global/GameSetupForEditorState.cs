using System;
using GameCore.Configs.Global.Game;
using GameCore.Enums.Global;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Services.Global;
using UnityEngine.SceneManagement;

namespace GameCore.StateMachine
{
    public class GameSetupForEditorState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameSetupForEditorState(
            IGameStateMachine gameStateMachine,
            IConfigsProvider configsProvider,
            IScenesLoaderService scenesLoaderService
        )
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

        private SceneName _startScene;
        private bool _loadStartScene;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _scenesLoaderService.OnSceneLoadedEvent += OnSceneLoaded;

            if (!TryLoadStartScene())
                CheckSceneState();
        }

        public void Exit() =>
            _scenesLoaderService.OnSceneLoadedEvent -= OnSceneLoaded;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadStartScene()
        {
            LoadScene(_startScene);
        }

        private void CheckSceneState()
        {
            if (IsSceneMatches(SceneName.Bootstrap))
            {
                EnterLoadFactoriesWarmUpState();
                return;
            }

            if (IsSceneMatches(SceneName.FactoriesWarmUp))
            {
                EnterFactoriesWarmUpState();
                return;
            }

            if (IsSceneMatches(SceneName.MainMenu))
            {
                EnterPrepareMainMenuState();
                return;
            }

            if (IsSceneMatches(SceneName.Gameplay))
                EnterPrepareGameplaySceneState();
        }

        private void LoadScene(SceneName sceneName) =>
            _scenesLoaderService.LoadScene(sceneName, isNetwork: false);

        private bool TryLoadStartScene()
        {
            bool useStartScene = _gameConfig.UseStartScene;

            if (!useStartScene)
                return false;

            _startScene = _gameConfig.StartScene;

            bool forceLoadBootstrapScene = _gameConfig.ForceLoadBootstrapScene;
            SceneName sceneToLoad = forceLoadBootstrapScene ? SceneName.Bootstrap : _startScene;

            string activeSceneNameString = SceneManager.GetActiveScene().name;
            var activeSceneName = (SceneName)Enum.Parse(typeof(SceneName), activeSceneNameString);
            bool skipLoading = activeSceneName == sceneToLoad;

            if (skipLoading)
                return false;

            if (sceneToLoad != _startScene)
                _loadStartScene = true;

            LoadScene(sceneToLoad);

            return true;
        }

        private void EnterLoadFactoriesWarmUpState() =>
            _gameStateMachine.ChangeState<LoadFactoriesWarmUpState>();

        private void EnterFactoriesWarmUpState() =>
            _gameStateMachine.ChangeState<FactoriesWarmUpState>();

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();

        private void EnterPrepareGameplaySceneState() =>
            _gameStateMachine.ChangeState<PrepareGameplaySceneState>();

        private static bool IsSceneMatches(SceneName sceneName)
        {
            string name = SceneManager.GetActiveScene().name;
            bool isSceneMainMenu = string.Equals(a: name, b: sceneName.ToString());
            return isSceneMainMenu;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSceneLoaded()
        {
            if (_loadStartScene)
            {
                _loadStartScene = false;
                LoadScene(_startScene);
                return;
            }

            CheckSceneState();
        }
    }
}