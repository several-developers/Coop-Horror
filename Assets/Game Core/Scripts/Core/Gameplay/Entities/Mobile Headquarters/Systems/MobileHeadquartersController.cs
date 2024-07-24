using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Network;
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

        public void InitAll()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent += OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent += OnOpenLocationsSelectionMenu;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent += OnCompleteQuests;

            SimpleButton loadMarketButton = _references.LoadMarketButton;
            loadMarketButton.OnTriggerEvent += OnLoadMarket;

            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent += OnInteractWithMainLever;
            loadLocationLever.OnEnabledEvent += OnMainLeverPulled;
        }

        public void DespawnAll()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

            SimpleButton openQuestsSelectionMenuButton = _references.OpenQuestsSelectionMenuButton;
            openQuestsSelectionMenuButton.OnTriggerEvent -= OnOpenQuestsSelectionMenu;

            SimpleButton openLocationsSelectionMenuButton = _references.OpenLocationsSelectionMenuButton;
            openLocationsSelectionMenuButton.OnTriggerEvent -= OnOpenLocationsSelectionMenu;

            SimpleButton completeQuestsButton = _references.CompleteQuestsButton;
            completeQuestsButton.OnTriggerEvent -= OnCompleteQuests;
            
            SimpleButton loadMarketButton = _references.LoadMarketButton;
            loadMarketButton.OnTriggerEvent -= OnLoadMarket;

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
            switch (gameState)
            {
                // Под вопросом это
                case GameState.ArrivedAtTheRoad:
                    _mobileHeadquartersEntity.PathMovement.ToggleArrived(isArrived: false);
                    _mobileHeadquartersEntity.ChangeToTheRoadPath();
                    break;

                case GameState.CycleMovement:
                    EnableMainLever();
                    break;

                case GameState.ReadyToLeaveTheLocation:
                    EnableMainLever();

                    if (NetworkHorror.IsTrueServer)
                        _mobileHeadquartersEntity.ToggleDoorServerRpc(isOpen: true);
                    break;

                case GameState.HeadingToTheRoad:
                    if (NetworkHorror.IsTrueServer)
                        _mobileHeadquartersEntity.ToggleDoorServerRpc(isOpen: false);
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
            GameState gameState = _mobileHeadquartersEntity.GameState;

            switch (gameState)
            {
                case GameState.CycleMovement:
                    bool containsExpiredQuests = _questsManagerDecorator.ContainsExpiredQuests();

                    if (containsExpiredQuests)
                    {
                        _mobileHeadquartersEntity.SendOpenGameOverWarningMenu();
                    }
                    else
                    {
                        _gameManagerDecorator.LoadSelectedLocation();
                    }

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

        private void OnCompleteQuests()
        {
            _mobileHeadquartersEntity.PlayCompleteQuestsButtonAnimationServerRpc();
            _questsManagerDecorator.CompleteQuests();
            _gameManagerDecorator.ChangeGameState(GameState.QuestsRewarding);
        }

        private void OnLoadMarket() =>
            _gameManagerDecorator.LoadLocation(SceneName.Market);

        private void OnInteractWithMainLever()
        {
            MobileHQMainLever mainLever = _references.MainLever;
            mainLever.InteractLogic();

            _mobileHeadquartersEntity.MainLeverAnimationServerRpc();
        }

        private void OnMainLeverPulled() => MainLeverLogic();
    }
}