using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
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
        private void Construct(ILevelProviderObserver levelProviderObserver) =>
            _levelProviderObserver = levelProviderObserver;

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

        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor Floor => _floor;
        public bool IsInStairsLocation => _isInStairsLocation;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;
        
        private ILevelProviderObserver _levelProviderObserver;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            FindDungeonRoot();
            RegisterSurfaceFireExit();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.TeleportToFireExit(_floor, _isInStairsLocation);
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
            
            bool isFound = transform.parent.parent.TryGetComponent(out DungeonRoot dungeonRoot);

            if (!isFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }

            _floor = dungeonRoot.Floor;
            dungeonRoot.AddFireExitToLevelManager(fireExit: this);
        }

        private void RegisterSurfaceFireExit()
        {
            if (!_isSurfaceFireExit)
                return;

            _levelProviderObserver.RegisterOtherFireExit(Floor.Surface, fireExit: this);
        }
    }
}