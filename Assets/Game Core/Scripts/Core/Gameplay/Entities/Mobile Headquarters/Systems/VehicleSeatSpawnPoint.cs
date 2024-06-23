using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class VehicleSeatSpawnPoint : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _yOffset = 1.1f;

        [SerializeField]
        private bool _drawGizmos;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawGizmos => _drawGizmos;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<VehicleSeatSpawnPoint> AllSpawnPoints = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            AllSpawnPoints.Add(item: this);

        private void OnDestroy() =>
            AllSpawnPoints.Remove(item: this);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector3 GetSpawnPosition()
        {
            Vector3 spawnPosition = transform.localPosition;
            spawnPosition.y += _yOffset;
            return spawnPosition;
        }

        public static bool GetRandomSpawnPoint(out VehicleSeatSpawnPoint result)
        {
            int spawnPointsAmount = AllSpawnPoints.Count;

            if (spawnPointsAmount == 0)
            {
                result = null;
                return false;
            }
            
            int randomIndex = Random.Range(0, spawnPointsAmount);
            result = AllSpawnPoints[randomIndex];
            return true;
        }
    }
}