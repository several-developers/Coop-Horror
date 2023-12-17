using GameCore.Gameplay;
using GameCore.Infrastructure.Services.Global.Data;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadDataState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadDataState(IGameStateMachine gameStateMachine, IGameSettingsDataService gameSettingsDataService)
        {
            _gameStateMachine = gameStateMachine;
            _gameSettingsDataService = gameSettingsDataService;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IGameSettingsDataService _gameSettingsDataService;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SetupGameSettings();
            //EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupGameSettings()
        {
            float soundVolume = _gameSettingsDataService.GetSoundVolume();
            float musicVolume = _gameSettingsDataService.GetMusicVolume();
        }

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}