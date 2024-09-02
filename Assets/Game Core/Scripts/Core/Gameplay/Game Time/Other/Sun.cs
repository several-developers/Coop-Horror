using GameCore.Observers.Gameplay.Time;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class Sun : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeObserver timeObserver) =>
            _timeObserver = timeObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Light _light;

        // FIELDS: --------------------------------------------------------------------------------

        private ITimeObserver _timeObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeObserver.OnTimeUpdatedEvent += OnTimeUpdated;

        private void OnDestroy() =>
            _timeObserver.OnTimeUpdatedEvent -= OnTimeUpdated;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetColor(Color color) =>
            _light.color = color;

        public void SetIntensity(float value)
        {
            value = Mathf.Max(a: value, b: 0f);
            _light.intensity = value;
        }

        public float GetIntensity() =>
            _light.intensity;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateSunRotation()
        {
            float timeOfDay = _timeObserver.GetDateTimeNormalized();

            Quaternion sunRotation = transform.rotation;
            float x = Mathf.Lerp(-90f, 270f, timeOfDay);
            float y = sunRotation.y;
            float z = sunRotation.z;

            transform.rotation = Quaternion.Euler(x, y, z);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime) => UpdateSunRotation();
    }
}