using GameCore.Gameplay;
using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;
using GameCore.Infrastructure.Services.Global.Data;

namespace GameCore.Infrastructure.StateMachine
{
    public class LoadDataState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LoadDataState(IGameStateMachine gameStateMachine, IGameSettingsDataService gameSettingsDataService,
            IDataProvider dataProvider)
        {
            _gameStateMachine = gameStateMachine;
            _gameSettingsDataService = gameSettingsDataService;
            _dataProvider = dataProvider;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IGameSettingsDataService _gameSettingsDataService;
        private readonly IDataProvider _dataProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
#if UNITY_EDITOR
            SetupDataEditor();
#endif
            
            SetupGameSettings();

#if UNITY_EDITOR
            EnterGameSetupForEditorState();
#else
            EnterLoadFactoriesWarmUpState();
#endif
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

#if UNITY_EDITOR
        private void SetupDataEditor()
        {
            DataManager dataManager = _dataProvider.GetDataManager();
            CustomEditors.DataEditor.SetDataManager(dataManager);
        }
#endif
        
        private void SetupGameSettings()
        {
            float soundVolume = _gameSettingsDataService.GetSoundVolume();
            float musicVolume = _gameSettingsDataService.GetMusicVolume();
        }

#if UNITY_EDITOR
        private void EnterGameSetupForEditorState() =>
            _gameStateMachine.ChangeState<GameSetupForEditorState>();
#endif

        private void EnterLoadFactoriesWarmUpState() =>
            _gameStateMachine.ChangeState<LoadFactoriesWarmUpState>();
    }
}