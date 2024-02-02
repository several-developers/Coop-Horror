using System.Collections;
using System.Collections.Generic;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Enums;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

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

        // FIELDS: --------------------------------------------------------------------------------

        private DungeonGeneratorConfigMeta _dungeonGeneratorConfig;
        private Coroutine _generatorCO;

        private List<RoomZone> _roomZonesInstances = new();

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
            CreateEnterRoom();
            yield return null;

            Vector2Int roomsAmount = _dungeonGeneratorConfig.RoomsAmount;
            int targetRoomsAmount = Random.Range(roomsAmount.x, roomsAmount.y + 1);
            int currentRoomsAmount = targetRoomsAmount;
            int corridorSpawnChance = _dungeonGeneratorConfig.CorridorSpawnChance;

            RoomZone lastRoomZone = _roomZonesInstances[0]; // Enter room
            
            while (currentRoomsAmount > 0)
            {
                bool spawnCorridor = GlobalUtilities.IsRandomSuccessful(corridorSpawnChance);
                DungeonRoomType roomType;

                if (spawnCorridor)
                    roomType = GetRandomCorridorRoomType();
                else
                    roomType = GetRandomRoomType();

                Vector3 spawnPosition = lastRoomZone.GetRandomDoorPosition();
                
                CreateRoomZone(roomType, spawnPosition);

                if (spawnCorridor)
                    continue;
                
                currentRoomsAmount--;
            }
            
            yield return null;
        }

        private void CreateEnterRoom()
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

            RoomZone roomZoneInstance = Instantiate(_roomZonePrefab, position, Quaternion.identity, _dungeonRoot);
            roomZoneInstance.Setup(roomSettings);
        }
        
        private bool TryGetRoomSettings(DungeonRoomType roomType, out RoomSettingsMeta roomSettings)
        {
            bool isRoomSettingsFound = _roomSettingsReferences.TryGetRoomSettings(roomType, out roomSettings);

            if (isRoomSettingsFound)
                return true;
            
            string errorLog = Log.HandleLog($"Room settings for <gb>{roomType}</gb> <rb>not found</rb>!");
            Debug.LogError(errorLog);

            return false;
        }

        private static DungeonRoomType GetRandomRoomType()
        {
            List<DungeonRoomType> roomTypes = new()
            {
                DungeonRoomType.TwoWaysRoom1
            };

            int randomIndex = Random.Range(0, roomTypes.Count);
            return roomTypes[randomIndex];
        }
        
        private static DungeonRoomType GetRandomCorridorRoomType()
        {
            List<DungeonRoomType> roomTypes = new()
            {
                DungeonRoomType.Corridor1,
                DungeonRoomType.Corridor2
            };

            int randomIndex = Random.Range(0, roomTypes.Count);
            return roomTypes[randomIndex];
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