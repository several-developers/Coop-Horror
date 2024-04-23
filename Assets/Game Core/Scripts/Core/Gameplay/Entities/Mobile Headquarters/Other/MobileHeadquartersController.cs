using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
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
            _rpcHandlerDecorator = mobileHeadquartersEntity.RpcHandlerDecorator;
            _gameManagerDecorator = mobileHeadquartersEntity.GameManagerDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly MobileHeadquartersReferences _references;
        private readonly IRpcHandlerDecorator _rpcHandlerDecorator;
        private readonly IGameManagerDecorator _gameManagerDecorator;

        private bool _ignoreMainLeverEvents = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void InitServerAndClient()
        {
            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent += OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent += OnMainLeverPulled;
        }
        
        public void InitServer()
        {
        }

        public void InitClient()
        {
        }
        
        public void DespawnServerAndClient()
        {
            MobileHQMainLever loadLocationLever = _references.MainLever;
            loadLocationLever.OnInteractEvent -= OnInteractWithLoadLocationLever;
            loadLocationLever.OnEnabledEvent -= OnMainLeverPulled;
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

        private void LoadLocation()
        {
            SceneName locationName = _gameManagerDecorator.GetSelectedLocation();
            _rpcHandlerDecorator.LoadLocation(locationName);
        }

        private void StartLeavingLocation() =>
            _rpcHandlerDecorator.StartLeavingLocation();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnInteractWithLoadLocationLever() =>
            _mobileHeadquartersEntity.LoadLocationServerRpc();

        private void OnMainLeverPulled()
        {
            LocationState locationState = _gameManagerDecorator.GetLocationState();
            bool isInRoadLocation = locationState == LocationState.Road;

            if (isInRoadLocation)
                LoadLocation();
            else
                StartLeavingLocation();
        }
    }
}