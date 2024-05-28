using System.Collections.Generic;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Network.Other
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private int _playerIndex;

        [SerializeField, Min(0)]
        private float _radius = 0.5f;

        [SerializeField, Min(0)]
        private float _yOffset = 1.1f;

        [SerializeField]
        private bool _drawGizmos;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int PlayerIndex => _playerIndex;
        public float Radius => _radius;
        public bool DrawGizmos => _drawGizmos;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<PlayerSpawnPoint> SpawnPointsList = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            SpawnPointsList.Add(item: this);

        private void OnDestroy() =>
            SpawnPointsList.Remove(item: this);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector3 GetRandomPosition()
        {
            Vector3 spawnPosition = transform.GetRandomLocalPosition(_radius);
            spawnPosition.y += _yOffset;
            return spawnPosition;
        }

        public static bool GetRandomSpawnPoint(out PlayerSpawnPoint spawnPoint)
        {
            int spawnPointsAmount = SpawnPointsList.Count;

            if (spawnPointsAmount == 0)
            {
                spawnPoint = null;
                return false;
            }

            int randomIndex = Random.Range(0, spawnPointsAmount);
            spawnPoint = SpawnPointsList[randomIndex];
            return true;
        }
    }
}