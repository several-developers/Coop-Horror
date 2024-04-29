using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Quests;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class MobileHeadquartersController : INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MobileHeadquartersController(MobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _references = mobileHeadquartersEntity.References;
            _gameManagerDecorator = mobileHeadquartersEntity.GameManagerDecorator;
            _questsManagerDecorator = mobileHeadquartersEntity.QuestsManagerDecorator;
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
            loadLocationLever.OnInteractEvent += OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent += OnMainLeverPulled;

            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent += OnDoorOpened;
        }

        public void InitServer()
        {
        }

        public void InitClient()
        {
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
            loadLocationLever.OnInteractEvent -= OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent -= OnMainLeverPulled;

            AnimationObserver animationObserver = _references.AnimationObserver;
            animationObserver.OnDoorOpenedEvent -= OnDoorOpened;
        }

        public void DespawnServer()
        {
        }

        public void DespawnClient()
        {
        }

        public void ToggleDoorState(bool isOpen)
        {
            Animator animator = _references.Animator;
            animator.SetBool(id: AnimatorHashes.IsOpen, isOpen);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            GameObject deathCamera;

            switch (gameState)
            {
                // Под вопросом это
                case GameState.ArrivedAtTheRoad:
                    _mobileHeadquartersEntity.ToggleMovement(canMove: false);
                    _mobileHeadquartersEntity.ArrivedAtRoadLocation();
                    break;

                case GameState.ReadyToLeaveTheRoad:
                    MobileHQMainLever mainLever = _references.MainLever;
                    mainLever.InteractWithoutEvents(isLeverPulled: false);
                    mainLever.ToggleInteract(canInteract: true);
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
                    _gameManagerDecorator.ChangeGameState(GameState.ReadyToLeaveTheRoad);
                    break;

                case GameState.ReadyToLeaveTheRoad:
                    bool containsExpiredQuests = _questsManagerDecorator.ContainsExpiredQuests();

                    if (containsExpiredQuests)
                        _gameManagerDecorator.ChangeGameState(GameState.KillPlayersOnTheRoad);
                    else
                        _gameManagerDecorator.LoadSelectedLocation();

                    break;

                case GameState.ReadyToLeaveTheLocation:
                    _mobileHeadquartersEntity.StartLeavingLocationServerRpc();
                    _gameManagerDecorator.ChangeGameState(GameState.HeadingToTheRoad);
                    break;
            }
        }

        private void DoorOpenedLogic()
        {
            GameState gameState = _mobileHeadquartersEntity.GameState;

            if (gameState != GameState.ArrivedAtTheLocation)
                return;

            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractWithoutEvents(isLeverPulled: false);
            mainLever.ToggleInteract(canInteract: true);

            _gameManagerDecorator.ChangeGameState(GameState.ReadyToLeaveTheLocation, ownerOnly: true);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnOpenQuestsSelectionMenu() =>
            _mobileHeadquartersEntity.SendOpenQuestsSelectionMenu();

        private void OnOpenLocationsSelectionMenu() =>
            _mobileHeadquartersEntity.SendOpenLocationsSelectionMenu();

        private void OnCallDeliveryDrone() =>
            _mobileHeadquartersEntity.SendCallDeliveryDrone();

        private void OnCompleteQuests()
        {
            _questsManagerDecorator.CompleteQuests();
            _gameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }

        private void OnInteractWithLoadLocationLever() =>
            _mobileHeadquartersEntity.LoadLocationServerRpc();

        private void OnMainLeverPulled() => MainLeverLogic();

        private void OnDoorOpened() => DoorOpenedLogic();
    }
}