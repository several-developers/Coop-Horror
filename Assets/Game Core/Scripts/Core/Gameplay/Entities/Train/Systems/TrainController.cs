using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.Train;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
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
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

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
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

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

        public void ToggleDoorState(bool isOpen)
        {
            Animator animator = _references.Animator;
            animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);
        }

        public void EnableMainLever()
        {
            TrainMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: false);
            mainLever.ToggleInteract(canInteract: true);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                // Под вопросом это
                case GameState.ArrivedAtTheRoad:
                    _trainEntity.PathMovement.ToggleArrived(isArrived: false);
                    _trainEntity.ChangeToTheRoadPath();
                    break;

                case GameState.CycleMovement:
                    EnableMainLever();
                    break;

                case GameState.ReadyToLeaveTheLocation:
                    EnableMainLever();

                    if (NetworkHorror.IsTrueServer)
                        _trainEntity.ToggleDoorServerRpc(isOpen: true);
                    break;

                case GameState.HeadingToTheRoad:
                    if (NetworkHorror.IsTrueServer)
                        _trainEntity.ToggleDoorServerRpc(isOpen: false);
                    break;

                case GameState.ArrivedAtTheLocation:
                    ToggleDoorState(isOpen: true);
                    break;

                case GameState.KillPlayersOnTheRoad:
                    ToggleDoorState(isOpen: true);
                    break;

                case GameState.RestartGame:
                    ToggleDoorState(isOpen: false);
                    break;
            }
        }

        private void MainLeverLogic()
        {
            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();
            bool leaveLocation = currentLocation is LocationName.Market or not LocationName.Base;

            if (leaveLocation)
            {
                _gameManagerDecorator.SelectLocation(LocationName.Base);
                _trainEntity.StartLeavingLocationServerRpc();
            }
            else
            {
                bool containsExpiredQuests = _questsManagerDecorator.ContainsExpiredQuests();

                if (containsExpiredQuests)
                    _trainEntity.SendOpenGameOverWarningMenu();
                else
                    _gameManagerDecorator.LoadSelectedLocation();
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnOpenQuestsSelectionMenu()
        {
            _trainEntity.SendOpenQuestsSelectionMenu();
            _trainEntity.PlayQuestsButtonAnimationServerRpc();
        }

        private void OnCompleteQuests()
        {
            _trainEntity.PlayCompleteQuestsButtonAnimationServerRpc();
            _questsManagerDecorator.CompleteQuests();
            _gameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }

        private void OnOpenGameMap() =>
            _trainEntity.SendOpenGameMapEvent();

        private void OnInteractWithMainLever()
        {
            TrainMainLever mainLever = _references.MainLever;
            mainLever.InteractLogic();

            _trainEntity.MainLeverAnimationServerRpc();
        }

        private void OnMainLeverPulled() => MainLeverLogic();
    }
}