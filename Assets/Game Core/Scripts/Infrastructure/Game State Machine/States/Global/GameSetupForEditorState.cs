using GameCore.Configs.Game;
using GameCore.Enums;
using GameCore.Gameplay;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Services.Global;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameSetupForEditorState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameSetupForEditorState(IGameStateMachine gameStateMachine, IConfigsProvider configsProvider,
            IScenesLoaderService scenesLoaderService)
        {
            _gameStateMachine = gameStateMachine;
            _configsProvider = configsProvider;
            _scenesLoaderService = scenesLoaderService;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IConfigsProvider _configsProvider;
        private readonly IScenesLoaderService _scenesLoaderService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => CheckGameConfig();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckGameConfig()
        {
            GameConfigMeta gameConfig = _configsProvider.GetGameConfig();
            bool useStartScene = gameConfig.UseStartScene;

            if (!useStartScene)
                return;

            SceneName startScene = gameConfig.StartScene;
            bool forceLoadBootstrapScene = gameConfig.ForceLoadBootstrapScene;
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
    }
}