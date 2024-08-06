using GameCore.Observers.Gameplay.Level;
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

        // FIELDS: --------------------------------------------------------------------------------
        
        private ILevelObserver _levelObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SendLocationLoaded();

        private void OnDestroy() => SendLocationUnloaded();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendLocationLoaded() =>
            _levelObserver.LocationLoaded();

        private void SendLocationUnloaded() =>
            _levelObserver.LocationUnloaded();
    }
}