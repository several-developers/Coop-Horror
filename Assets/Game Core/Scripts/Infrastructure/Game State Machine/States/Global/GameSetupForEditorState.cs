using System;
using GameCore.Configs.Global.Game;
using GameCore.Enums.Global;
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

            CheckSceneState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGameConfig()
        {
            SceneName startScene = _gameConfig.StartScene;
            bool forceLoadBootstrapScene = _gameConfig.ForceLoadBootstrapScene;
            bool loadOnlyBootstrapScene = forceLoadBootstrapScene && startScene == SceneName.Bootstrap;

            if (forceLoadBootstrapScene)
                LoadScene(SceneName.Bootstrap, OnSceneLoaded);
            else
                LoadScene(startScene, CheckSceneState);
            
            // LOCAL METHODS: -----------------------------

            void OnSceneLoaded()
            {
                if (loadOnlyBootstrapScene)
                    return;

                LoadScene(startScene, CheckSceneState);
            }
        }

        private void CheckSceneState()
        {
            if (IsSceneMatches(SceneName.Bootstrap))
            {
                EnterLoadMainMenuState();
                return;
            }
            
            if (IsSceneMatches(SceneName.MainMenu))
            {
                EnterPrepareMainMenuState();
                return;
            }

            if (IsSceneMatches(SceneName.Gameplay))
                EnterGameplaySceneState();
        }
        
        private void LoadScene(SceneName sceneName, Action callback = null) =>
            _scenesLoaderService.LoadScene(sceneName, callback);

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
        
        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();
        
        private void EnterGameplaySceneState() =>
            _gameStateMachine.ChangeState<GameplaySceneState>();

        private static bool IsSceneMatches(SceneName sceneName)
        {
            string name = SceneManager.GetActiveScene().name;
            bool isSceneMainMenu = string.Equals(name, sceneName.ToString());
            return isSceneMainMenu;
        }
    }
}