using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.Train;
using GameCore.Gameplay.Systems.Quests;

namespace GameCore.Gameplay.Entities.Level.Train
{
    public class TrainController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TrainController(TrainEntity trainEntity)
        {
            _trainEntity = trainEntity;
            _references = trainEntity.References;
            _gameManagerDecorator = trainEntity.GameManagerDecorator;
            _questsManagerDecorator = trainEntity.QuestsManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly TrainEntity _trainEntity;
        private readonly TrainReferences _references;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IQuestsManagerDecorator _questsManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitAll()
        {
            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent += OnOpenQuestsSelectionMenu;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent += OnCompleteQuests;

            SimpleButton openGameMapButton = _references.OpenGameMapButton;
            openGameMapButton.OnTriggerEvent += OnOpenGameMap;

            TrainMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent += OnInteractWithMainLever;
            loadLocationLever.OnEnabledEvent += OnMainLeverPulled;
        }

        public void DespawnAll()
        {
            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent -= OnOpenQuestsSelectionMenu;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent -= OnCompleteQuests;

            SimpleButton openGameMapButton = _references.OpenGameMapButton;
            openGameMapButton.OnTriggerEvent -= OnOpenGameMap;

            TrainMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent -= OnInteractWithMainLever;
            loadLocationLever.OnEnabledEvent -= OnMainLeverPulled;
        }

        public void ToggleMainLeverState(bool isEnabled)
        {
            TrainMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: !isEnabled);
            mainLever.ToggleInteract(canInteract: isEnabled);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MainLeverLogic()
        {
            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();
            bool leaveLocation = currentLocation is LocationName.Market or not LocationName.Base;

            if (leaveLocation)
            {
                _trainEntity.StartTrainRpc();
                _trainEntity.ToggleMainLeverState(isEnabled: false);
            }
            else
            {
                bool containsExpiredQuests = _questsManagerDecorator.ContainsExpiredQuests();

                if (containsExpiredQuests)
                {
                    _trainEntity.SendOpenGameOverWarningMenu();
                }
                else
                {
                    _gameManagerDecorator.LoadSelectedLocation();
                    _trainEntity.ToggleMainLeverState(isEnabled: false);
                }
            }
            
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOpenQuestsSelectionMenu() =>
            _trainEntity.SendOpenQuestsSelectionMenu();

        private void OnCompleteQuests()
        {
            _questsManagerDecorator.CompleteQuests();
            _gameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }

        private void OnOpenGameMap() =>
            _trainEntity.SendOpenGameMapEvent();

        private void OnInteractWithMainLever()
        {
            TrainMainLever mainLever = _references.MainLever;
            mainLever.InteractLogic();

            _trainEntity.PlayMainLeverPullAnimationRpc();
        }

        private void OnMainLeverPulled() => MainLeverLogic();
    }
}