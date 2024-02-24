using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class FireExit : MonoBehaviour, IInteractable
    {
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
                string errorLog = Log.HandleLog($"<gb>Dungeon Root</gb> component <rb>not found</rb>!");
                Debug.LogError(errorLog);
                return;
            }

            _floor = dungeonRoot.Floor;
            dungeonRoot.AddFireExitToLevelManager(fireExit: this);
        }

        private void RegisterSurfaceFireExit()
        {
            if (!_isSurfaceFireExit)
                return;

            NetworkServiceLocator networkServiceLocator = NetworkServiceLocator.Get();
            ILevelManager levelManager = networkServiceLocator.GetLevelManager();
            levelManager.AddOtherFireExit(Floor.Surface, fireExit: this);
        }
    }
}