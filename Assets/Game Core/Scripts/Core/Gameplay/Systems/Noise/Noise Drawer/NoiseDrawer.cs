using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Noise
{
    public class NoiseDrawer : MonoBehaviour
    {
#if UNITY_EDITOR

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _gizmosDuration = 0.2f;
        
        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<NoiseData> _noisesData = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Draw(Vector3 noisePosition, float noiseRange)
        {
            NoiseData data = new(noisePosition, noiseRange, timeLeft: _gizmosDuration);
            _noisesData.Add(data);
        }

        public void RemoveNoiseData(int index) =>
            _noisesData.RemoveAt(index);

        public IReadOnlyList<NoiseData> GetAllNoiseData() => _noisesData;

        // INNER CLASSES: -------------------------------------------------------------------------

        public class NoiseData
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public NoiseData(Vector3 noisePosition, float noiseRange, float timeLeft)
            {
                NoisePosition = noisePosition;
                NoiseRange = noiseRange;
                _timeLeft = timeLeft;
            }

            // FIELDS: --------------------------------------------------------------------------------

            public Vector3 NoisePosition { get; }
            public float NoiseRange { get; }
            
            private float _timeLeft;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public bool DecreaseTime(float deltaTime)
            {
                _timeLeft -= deltaTime;
                bool isTimeOver = _timeLeft <= 0.0f;
                return isTimeOver;
            }
        }

#endif
    }
}