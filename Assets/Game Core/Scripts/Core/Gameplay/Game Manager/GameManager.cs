using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Level;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManager : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator, ILevelObserver levelObserver)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _levelObserver = levelObserver;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const SceneName DefaultLocation = SceneName.Desert;
        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private readonly NetworkVariable<SceneName> _selectedLocation =
            new(value: DefaultLocation, writePerm: OwnerPermission);

        private readonly NetworkVariable<LocationState> _locationState = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private ILevelObserver _levelObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _selectedLocation.OnValueChanged += OnSelectedLocationChanged;
            
            _gameManagerDecorator.OnSelectLocationInnerEvent += SelectLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent += GetSelectedLocation;
            _gameManagerDecorator.OnGetLocationStateInnerEvent += GetLocationState;
        }

        public override void OnDestroy()
        {
            _selectedLocation.OnValueChanged -= OnSelectedLocationChanged;
            
            _gameManagerDecorator.OnSelectLocationInnerEvent -= SelectLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent -= GetSelectedLocation;
            _gameManagerDecorator.OnGetLocationStateInnerEvent -= GetLocationState;
            
            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void InitServerAndClient() =>
            _locationState.OnValueChanged += OnInLocationStateChanged;

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _levelObserver.OnLocationLoadedEvent += OnLocationLoaded;
            _levelObserver.OnLocationLeftEvent += OnLocationLeft;
        }

        public void InitClient()
        {
            if (IsOwner)
                return;
        }

        public void DespawnServerAndClient() =>
            _locationState.OnValueChanged -= OnInLocationStateChanged;

        public void DespawnServer()
        {
            if (!IsOwner)
                return;
            
            _levelObserver.OnLocationLoadedEvent -= OnLocationLoaded;
            _levelObserver.OnLocationLeftEvent -= OnLocationLeft;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SelectLocation(SceneName sceneName) =>
            _selectedLocation.Value = sceneName;

        private SceneName GetSelectedLocation() =>
            _selectedLocation.Value;

        private LocationState GetLocationState() =>
            _locationState.Value;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }
        
        private void OnSelectedLocationChanged(SceneName previousValue, SceneName newValue) =>
            _gameManagerDecorator.SelectedLocationChanged(newValue);

        private void OnLocationLoaded()
        {
            _locationState.Value = LocationState.Game;
        }
        
        private void OnLocationLeft()
        {
            _locationState.Value = LocationState.Road;
        }

        private void OnInLocationStateChanged(LocationState previousValue, LocationState newValue) =>
            _gameManagerDecorator.LocationStateChanged(locationState: newValue);
    }
}