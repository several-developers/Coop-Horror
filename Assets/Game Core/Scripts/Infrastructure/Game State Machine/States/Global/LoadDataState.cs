using GameCore.Gameplay;
using GameCore.Infrastructure.Services.Global;
using GameCore.Infrastructure.Services.Global.Data;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadDataState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadDataState(IGameStateMachine gameStateMachine, IScenesLoaderService scenesLoaderService,
            IGameSettingsDataService gameSettingsDataService)
        {
            _gameStateMachine = gameStateMachine;
            _scenesLoaderService = scenesLoaderService;
            _gameSettingsDataService = gameSettingsDataService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IScenesLoaderService _scenesLoaderService;
        private readonly IGameSettingsDataService _gameSettingsDataService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SetupGameSettings();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupGameSettings()
        {
            float soundVolume = _gameSettingsDataService.GetSoundVolume();
            float musicVolume = _gameSettingsDataService.GetMusicVolume();
        }
    }
}