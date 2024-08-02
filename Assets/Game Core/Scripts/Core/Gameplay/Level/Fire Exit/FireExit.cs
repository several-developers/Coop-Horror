using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.LevelManager;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level
{
    public class FireExit : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IFireExitsManager fireExitsManager, ILevelProviderObserver levelProviderObserver)
        {
            _levelProviderObserver = levelProviderObserver;
            _fireExitsManager = fireExitsManager;
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

        private IFireExitsManager _fireExitsManager;
        private ILevelProviderObserver _levelProviderObserver;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            FindDungeonRoot();
            RegisterStairsFireExit();
            RegisterLocationSurfaceFireExit();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
            if (entity == null)
                return;

            bool isPlayer = entity.GetType() == typeof(PlayerEntity);

            if (!isPlayer)
            {
                bool isTeleportableEntity = entity is ITeleportableEntity;

                if (!isTeleportableEntity)
                    return;

                var teleportableEntity = (ITeleportableEntity)entity;
                _fireExitsManager.TeleportEntityToFireExit(teleportableEntity, _floor, _isInStairsLocation);
            }
            else
                _fireExitsManager.TeleportLocalPlayerToFireExit(_floor, _isInStairsLocation);
        }

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

            DungeonRoot dungeonRoot = null;
            Transform parent = transform.parent;
            bool isParentFound = parent != null;
            bool isDungeonRootFound = false;
            int iterations = 0;

            while (isParentFound)
            {
                isDungeonRootFound = parent.TryGetComponent(out dungeonRoot);

                if (isDungeonRootFound)
                    break;
                
                parent = parent.parent;
                isParentFound = parent != null;

                if (iterations > 100)
                {
                    Debug.LogError("Infinity loop!");
                    break;
                }
                
                iterations++;
            }

            if (!isDungeonRootFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }

            _floor = dungeonRoot.Floor;
            _levelProviderObserver.RegisterOtherFireExit(_floor, fireExit: this);
        }

        private void RegisterStairsFireExit()
        {
            if (!_isInStairsLocation || _levelProviderObserver == null)
                return;

            _levelProviderObserver.RegisterStairsFireExit(_floor, fireExit: this);
        }

        private void RegisterLocationSurfaceFireExit()
        {
            if (_isInStairsLocation || _floor != Floor.Surface || _levelProviderObserver == null)
                return;
            
            _levelProviderObserver.RegisterOtherFireExit(_floor, fireExit: this);
        }
    }
}