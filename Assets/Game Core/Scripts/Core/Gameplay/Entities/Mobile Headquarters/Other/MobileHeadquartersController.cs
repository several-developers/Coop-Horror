using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
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
            MobileHQMainLever mainLever = _references.MainLever;

            switch (gameState)
            {
                case GameState.ReadyToLeaveTheRoad:
                    mainLever.InteractWithoutEvents(isLeverPulled: false);
                    mainLever.ToggleInteract(canInteract: true);
                    break;

                case GameState.ArrivedAtTheLocation:
                    ToggleDoorState(isOpen: true);
                    break;
                
                case GameState.KillPlayersOnTheRoad:
                    ToggleDoorState(isOpen: true);
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

            if (_mobileHeadquartersEntity.IsOwner)
                _gameManagerDecorator.ChangeGameState(GameState.ReadyToLeaveTheLocation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnInteractWithLoadLocationLever() =>
            _mobileHeadquartersEntity.LoadLocationServerRpc();

        private void OnMainLeverPulled() => MainLeverLogic();

        private void OnDoorOpened() => DoorOpenedLogic();
    }
}