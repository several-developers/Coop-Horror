using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Systems.Spawners
{
    public class DebugMonstersSpawner : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMonstersFactory monstersFactory) =>
            _monstersFactory = monstersFactory;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private MonsterType _monsterType;

        [SerializeField, Min(1)]
        private int _spawnAmount = 1;

        [SerializeField]
        private float _spawnOffsetY = 5f;

        [SerializeField, Min(0f)]
        private float _radius = 1f;

        [SerializeField]
        private bool _spawnAtStart = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float Radius => _radius;

        // FIELDS: --------------------------------------------------------------------------------

        private IMonstersFactory _monstersFactory;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => TrySpawnMonsters();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetMonsterName() =>
            $"'Monster: {_monsterType}'";

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrySpawnMonsters()
        {
            if (!_spawnAtStart)
                return;

            if (!NetworkHorror.IsTrueServer)
                return;

            for (int i = 0; i < _spawnAmount; i++)
                SpawnMonster();
        }

        private void SpawnMonster()
        {
            Vector3 spawnPosition = GetSpawnPosition();

            var spawnParams = new SpawnParams<MonsterEntityBase>.Builder()
                .SetSpawnPosition(spawnPosition)
                .Build();

            _monstersFactory.CreateMonsterDynamic(_monsterType, spawnParams);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 thisPosition = transform.position;
            Vector3 spawnPosition = thisPosition;
            Vector2 randomInCircle = Random.insideUnitCircle * _radius;

            spawnPosition.x += randomInCircle.x;
            spawnPosition.y += _spawnOffsetY;
            spawnPosition.z += randomInCircle.y;

            bool hitGround = Physics.Raycast(origin: spawnPosition, direction: Vector3.down, out RaycastHit hitInfo,
                maxDistance: 100);
            
            return hitGround ? hitInfo.point : thisPosition;
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 35), DisableInEditorMode]
        private void DebugSpawnMonster() => SpawnMonster();
    }
}