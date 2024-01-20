using UnityEngine;

namespace GameCore.Gameplay.Locations
{
    public class LocationManager : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static LocationManager _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static LocationManager Get() => _instance;
    }
}