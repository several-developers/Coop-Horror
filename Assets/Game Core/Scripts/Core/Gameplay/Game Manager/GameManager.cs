using GameCore.Enums.Global;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManager : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // FIELDS: --------------------------------------------------------------------------------

        private const SceneName DefaultLocation = SceneName.Desert;
        private const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;

        private readonly NetworkVariable<SceneName> _selectedLocation =
            new(value: DefaultLocation, writePerm: OwnerPermission);

        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _selectedLocation.OnValueChanged += OnSelectedLocationChanged;
            
            _gameManagerDecorator.OnSelectLocationInnerEvent += SelectLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent += GetSelectedLocation;
        }

        public override void OnDestroy()
        {
            _selectedLocation.OnValueChanged -= OnSelectedLocationChanged;
            
            _gameManagerDecorator.OnSelectLocationInnerEvent -= SelectLocation;
            _gameManagerDecorator.OnGetSelectedLocationInnerEvent -= GetSelectedLocation;
            
            base.OnDestroy();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SelectLocation(SceneName sceneName) =>
            _selectedLocation.Value = sceneName;

        private SceneName GetSelectedLocation() =>
            _selectedLocation.Value;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedLocationChanged(SceneName previousValue, SceneName newValue) =>
            _gameManagerDecorator.SelectedLocationChanged(newValue);
    }
}