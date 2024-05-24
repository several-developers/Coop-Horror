using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Quests;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MobileHeadquartersController(MobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _references = mobileHeadquartersEntity.References;
            _gameManagerDecorator = mobileHeadquartersEntity.GameManagerDecorator;
            _questsManagerDecorator = mobileHeadquartersEntity.QuestsManagerDecorator;

            //EnableMainLever();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly MobileHeadquartersReferences _references;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly IQuestsManagerDecorator _questsManagerDecorator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent += OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent += OnOpenLocationsSelectionMenu;

            SimpleButton callDeliveryDroneButton = _references.CallDeliveryDroneButton;
            callDeliveryDroneButton.OnTriggerEvent += OnCallDeliveryDrone;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent += OnCompleteQuests;

            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent += OnInteractWithMainLever;
            loadLocationLever.OnEnabledEvent += OnMainLeverPulled;
        }

        public void DespawnServerAndClient()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent -= OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent -= OnOpenLocationsSelectionMenu;

            SimpleButton callDeliveryDroneButton = _references.CallDeliveryDroneButton;
            callDeliveryDroneButton.OnTriggerEvent -= OnCallDeliveryDrone;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent -= OnCompleteQuests;

            MobileHQMainLever loadLocationLever = _references.MainLever;
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
            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: false);
            mainLever.ToggleInteract(canInteract: true);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            GameObject deathCamera;

            switch (gameState)
            {
                // Под вопросом это
                case GameState.ArrivedAtTheRoad:
                    _mobileHeadquartersEntity.PathMovement.ToggleArrived(isArrived: false);
                    _mobileHeadquartersEntity.ChangeToRoadPath();
                    break;

                case GameState.ReadyToLeaveTheRoad:
                case GameState.ReadyToLeaveTheLocation:
                    EnableMainLever();
                    break;

                case GameState.ArrivedAtTheLocation:
                    ToggleDoorState(isOpen: true);
                    break;

                case GameState.KillPlayersOnTheRoad:
                    deathCamera = _references.DeathCamera;
                    deathCamera.SetActive(true);

                    ToggleDoorState(isOpen: true);
                    break;

                case GameState.RestartGame:
                    deathCamera = _references.DeathCamera;
                    deathCamera.SetActive(false);

                    ToggleDoorState(isOpen: false);
                    break;
            }
        }

        private void MainLeverLogic()
        {
            GameState gameState = _mobileHeadquartersEntity.GameState;

            switch (gameState)
            {
                case GameState.WaitingForPlayers:
                    //_gameManagerDecorator.ChangeGameState(GameState.ReadyToLeaveTheRoad);
                    break;

                case GameState.ReadyToLeaveTheRoad:
                    bool containsExpiredQuests = _questsManagerDecorator.ContainsExpiredQuests();

                    if (containsExpiredQuests)
                        _mobileHeadquartersEntity.SendOpenGameOverWarningMenu();
                    else
                        _gameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.LeavingRoadLocation,
                            previousState: GameState.ReadyToLeaveTheRoad);
                    break;

                case GameState.ReadyToLeaveTheLocation:
                    _mobileHeadquartersEntity.StartLeavingLocationServerRpc();
                    
                    _gameManagerDecorator.ChangeGameStateWhenAllPlayersReady(newState: GameState.HeadingToTheRoad,
                        previousState: GameState.ReadyToLeaveTheLocation);
                    break;
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnOpenQuestsSelectionMenu()
        {
            _mobileHeadquartersEntity.SendOpenQuestsSelectionMenu();
            _mobileHeadquartersEntity.PlayQuestsButtonAnimationServerRpc();
        }

        private void OnOpenLocationsSelectionMenu()
        {
            _mobileHeadquartersEntity.SendOpenLocationsSelectionMenu();
            _mobileHeadquartersEntity.PlayLocationsButtonAnimationServerRpc();
        }

        private void OnCallDeliveryDrone()
        {
            _mobileHeadquartersEntity.SendCallDeliveryDrone();
            _mobileHeadquartersEntity.PlayDeliveryDroneButtonAnimationServerRpc();
        }

        private void OnCompleteQuests()
        {
            _mobileHeadquartersEntity.PlayCompleteQuestsButtonAnimationServerRpc();
            _questsManagerDecorator.CompleteQuests();
            _gameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }

        private void OnInteractWithMainLever()
        {
            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractLogic();

            _mobileHeadquartersEntity.MainLeverAnimationServerRpc();
        }

        private void OnMainLeverPulled() => MainLeverLogic();
    }
}