using System.Collections.Generic;
using Cinemachine;
using GameCore.Gameplay.Level.LocationsMechanics;
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
        private void Construct(ILevelObserver levelObserver) =>
            _levelObserver = levelObserver;
            
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _enterPath;
        
        [SerializeField, Required]
        private CinemachinePath _exitPath;

        [SerializeField, Required, Space(height: 5)]
        private List<LocationMechanic> _locationMechanics;

        // FIELDS: --------------------------------------------------------------------------------
        
        private static LocationManager _instance;
        
        private ILevelObserver _levelObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            SendLocationLoaded();
        }

        private void OnDestroy() => SendLocationUnloaded();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static LocationManager Get() => _instance;

        public CinemachinePath GetEnterPath() => _enterPath;

        public CinemachinePath GetExitPath() => _exitPath;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendLocationLoaded() =>
            _levelObserver.LocationLoaded();

        private void SendLocationUnloaded() =>
            _levelObserver.LocationUnloaded();
    }
}