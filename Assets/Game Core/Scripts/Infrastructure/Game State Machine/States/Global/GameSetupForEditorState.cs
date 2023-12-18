using System;
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

            void OnSceneLoaded()
            {
                if (loadOnlyBootstrapScene)
                    return;

                LoadScene(startScene, CheckSceneState);
            }
        }

        private void CheckSceneState()
        {
            if (IsCurrentSceneMainMenu())
            {
                EnterPrepareMainMenuState();
                return;
            }

            if (IsCurrentSceneGameplay())
                EnterGameplayState();
        }
        
        private void LoadScene(SceneName sceneName, Action callback = null) =>
            _scenesLoaderService.LoadScene(sceneName, callback);

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();
        
        private void EnterGameplayState() =>
            _gameStateMachine.ChangeState<GameplayState>();

        private static bool IsCurrentSceneMainMenu()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isSceneMainMenu = string.Equals(sceneName, SceneName.MainMenu.ToString());
            return isSceneMainMenu;
        }
        
        private static bool IsCurrentSceneGameplay()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isSceneGameplay = string.Equals(sceneName, SceneName.Gameplay.ToString());
            return isSceneGameplay;
        }
    }
}