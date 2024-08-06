using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.LevelManager;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Locations
{
    public class MetroDoor : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMetroManager metroManager, ILevelProviderObserver levelProviderObserver)
        {
            _metroManager = metroManager;
            _levelProviderObserver = levelProviderObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _placedAtSurface;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _teleportPoint;

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnInteractionStateChangedEvent;

        private IMetroManager _metroManager;
        private ILevelProviderObserver _levelProviderObserver;
        private bool _canInteract = true;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => RegisterMetroDoor();

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

            if (isPlayer)
            {
                _metroManager.TeleportLocalPlayer(_placedAtSurface);
                return;
            }

            bool isTeleportableEntity = entity is ITeleportableEntity;

            if (!isTeleportableEntity)
                return;

            var teleportableEntity = (ITeleportableEntity)entity;
            _metroManager.TeleportEntity(teleportableEntity, _placedAtSurface);
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent?.Invoke();
        }
        
        public Transform GetTeleportPoint() => _teleportPoint;

        public InteractionType GetInteractionType() =>
            InteractionType.MetroDoor;

        public bool CanInteract() => _canInteract;
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterMetroDoor() =>
            _levelProviderObserver.RegisterMetroDoor(metroDoor: this, _placedAtSurface);
    }
}