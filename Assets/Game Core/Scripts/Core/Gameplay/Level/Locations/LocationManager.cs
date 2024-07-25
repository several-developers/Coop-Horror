using Cinemachine;
using GameCore.Observers.Gameplay.Level;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILocationManagerDecorator locationManagerDecorator, ILevelObserver levelObserver)
        {
            _locationManagerDecorator = locationManagerDecorator;
            _levelObserver = levelObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _enterPath;
        
        [SerializeField, Required]
        private CinemachinePath _exitPath;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private ILocationManagerDecorator _locationManagerDecorator;
        private ILevelObserver _levelObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _locationManagerDecorator.OnGetEnterPathInnerEvent += GetEnterPath;
            _locationManagerDecorator.OnGetExitPathInnerEvent += GetExitPath;
        }

        private void Start() => SendLocationLoaded();

        private void OnDestroy()
        {
            SendLocationUnloaded();
            
            _locationManagerDecorator.OnGetEnterPathInnerEvent -= GetEnterPath;
            _locationManagerDecorator.OnGetExitPathInnerEvent -= GetExitPath;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendLocationLoaded() =>
            _levelObserver.LocationLoaded();

        private void SendLocationUnloaded() =>
            _levelObserver.LocationLeft();
        
        private CinemachinePath GetEnterPath() => _enterPath;
        
        private CinemachinePath GetExitPath() => _exitPath;
    }
}