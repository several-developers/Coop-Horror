using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.LevelManager;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels
{
    public class FireExit : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator, ILevelProviderObserver levelProviderObserver)
        {
            _rpcHandlerDecorator = rpcHandlerDecorator;
            _levelProviderObserver = levelProviderObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;

        [SerializeField]
        private bool _isInStairsLocation;

        [SerializeField]
        private bool _isSurfaceFireExit;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _teleportPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private IRpcHandlerDecorator _rpcHandlerDecorator;
        private ILevelProviderObserver _levelProviderObserver;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            FindDungeonRoot();
            RegisterStairsFireExit();
            RegisterLocationSurfaceFireExit();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Interact(PlayerEntity playerEntity = null) =>
            _rpcHandlerDecorator.TeleportToFireExit(_floor, _isInStairsLocation);

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent?.Invoke();
        }

        public Transform GetTeleportPoint() => _teleportPoint;

        public InteractionType GetInteractionType() =>
            InteractionType.FireExitDoor;

        public bool CanInteract() => _canInteract;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FindDungeonRoot()
        {
            if (_isInStairsLocation || _isSurfaceFireExit)
                return;

            bool isFound = transform.parent.parent.TryGetComponent(out DungeonRoot dungeonRoot);

            if (!isFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }

            _floor = dungeonRoot.Floor;
            _levelProviderObserver.RegisterOtherFireExit(_floor, fireExit: this);
        }

        private void RegisterStairsFireExit()
        {
            if (!_isInStairsLocation)
                return;

            _levelProviderObserver.RegisterStairsFireExit(_floor, fireExit: this);
        }

        private void RegisterLocationSurfaceFireExit()
        {
            if (_isInStairsLocation || _floor != Floor.Surface)
                return;
            
            _levelProviderObserver.RegisterOtherFireExit(_floor, fireExit: this);
        }
    }
}