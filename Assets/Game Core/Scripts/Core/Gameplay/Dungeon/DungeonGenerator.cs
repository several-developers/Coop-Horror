using System;
using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Enums;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider) =>
            _dungeonGeneratorConfig = gameplayConfigsProvider.GetDungeonGeneratorConfig();

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _dungeonRoot;

        [SerializeField, Required]
        private RoomZone _roomZonePrefab;

        [SerializeField, Space(height: 5)]
        private RoomSettingsReferences _roomSettingsReferences;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int LastRoomIndex => _lastRoomIndex;

        // FIELDS: --------------------------------------------------------------------------------

        private DungeonGeneratorConfigMeta _dungeonGeneratorConfig;
        private RoomZonesGeneratorLogic _roomZonesGeneratorLogic;
        private Coroutine _generatorCO;

        private List<RoomZone> _roomZonesInstances = new();
        private int _lastRoomIndex;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _roomZonesGeneratorLogic = new RoomZonesGeneratorLogic(dungeonGenerator: this, _dungeonGeneratorConfig);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CreateRoomZone(RoomSettingsMeta roomSettings, Vector3 position, Quaternion rotation)
        {
            _lastRoomIndex++;

            RoomZone roomZoneInstance = Instantiate(_roomZonePrefab, position, rotation, _dungeonRoot);
            roomZoneInstance.Setup(roomSettings, _lastRoomIndex);

            _roomZonesInstances.Add(roomZoneInstance);
        }

        public bool TryGetRoomSettings(DungeonRoomType roomType, out RoomSettingsMeta roomSettings)
        {
            bool isRoomSettingsFound = _roomSettingsReferences.TryGetRoomSettings(roomType, out roomSettings);

            if (isRoomSettingsFound)
                return true;

            string errorLog = Log.HandleLog($"Room settings for <gb>{roomType}</gb> <rb>not found</rb>!");
            //Debug.LogError(errorLog);

            return false;
        }

        public RoomZone GetLastRoomZone() => _roomZonesInstances[^1];

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StartGenerator()
        {
            if (_generatorCO != null)
                StopCoroutine(_generatorCO);

            IEnumerator routine = GeneratorLogic();
            _generatorCO = StartCoroutine(routine);
        }

        private void ClearDungeon()
        {
            foreach (RoomZone roomZone in _roomZonesInstances)
                Destroy(roomZone.gameObject);

            _roomZonesInstances.Clear();
        }

        private IEnumerator GeneratorLogic()
        {
            CreateEnterRoomZone();
            yield return null;

            Vector2Int roomsAmount = _dungeonGeneratorConfig.RoomsAmount;
            int targetRoomsAmount = Random.Range(roomsAmount.x, roomsAmount.y + 1);
            int currentRoomsAmount = targetRoomsAmount;

            int iterations = 0;

            Debug.Log($"Rooms amount: {targetRoomsAmount}");

            while (currentRoomsAmount > 0)
            {
                if (CheckInfinityLoop())
                    break;

                _roomZonesGeneratorLogic.HandleRoomSpawn();

                currentRoomsAmount--;

                yield return new WaitForSeconds(1f);
            }

            yield return null;

            // LOCAL METHODS: -----------------------------

            bool CheckInfinityLoop()
            {
                iterations++;

                if (iterations < 100)
                    return false;

                Debug.LogError("Infinity loop!");
                return true;
            }
        }

        private void CreateEnterRoomZone()
        {
            Vector3 spawnPosition = Vector3.zero;
            spawnPosition.y -= _dungeonGeneratorConfig.DungeonSpawnOffsetY;

            CreateRoomZone(DungeonRoomType.Enter, spawnPosition);
        }

        private void CreateRoomZone(DungeonRoomType roomType, Vector3 position)
        {
            bool isRoomSettingsFound = TryGetRoomSettings(roomType, out RoomSettingsMeta roomSettings);

            if (!isRoomSettingsFound)
                return;

            CreateRoomZone(roomSettings, position, Quaternion.identity);
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugStartGenerator()
        {
            ClearDungeon();
            StartGenerator();
        }

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugClearDungeon() => ClearDungeon();
    }
}