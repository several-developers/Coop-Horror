using System.Collections.Generic;
using GameCore.Configs.Gameplay.DungeonGenerator;
using GameCore.Enums;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    public class RoomZonesGeneratorLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RoomZonesGeneratorLogic(DungeonGenerator dungeonGenerator,
            DungeonGeneratorConfigMeta dungeonGeneratorConfig)
        {
            _dungeonGenerator = dungeonGenerator;
            _dungeonGeneratorConfig = dungeonGeneratorConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DungeonGenerator _dungeonGenerator;
        private readonly DungeonGeneratorConfigMeta _dungeonGeneratorConfig;

        private Dictionary<int, List<DoorwayDirection>> _busyRoomsDoorways = new();
        private List<DoorwayDirection> _availableDoorways = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void HandleRoomSpawn()
        {
            int lastRoomIndex = _dungeonGenerator.LastRoomIndex;

            RoomZone lastRoomZone = _dungeonGenerator.GetLastRoomZone();
            RoomSettingsMeta lastRoomSettings = lastRoomZone.GetRoomSettings();
            
            if (!GetRoomRandomDoorway(lastRoomSettings, out DoorwaySettings lastRoomDoorwaySettings))
                return;

            DoorwayConnectionType lastRoomConnectionType = lastRoomDoorwaySettings.ConnectionType;
            bool spawnHallway = false;

            switch (lastRoomConnectionType)
            {
                case DoorwayConnectionType.Hallway:
                    spawnHallway = true;
                    break;
                
                case DoorwayConnectionType.RoomOrHallway:
                    int hallwaySpawnChance = _dungeonGeneratorConfig.HallwaySpawnChance;
                    spawnHallway = GlobalUtilities.IsRandomSuccessful(hallwaySpawnChance);
                    break;
            }

            DungeonRoomType roomType;

            if (spawnHallway)
                roomType = GetRandomHallwayType();
            else
                roomType = GetRandomRoomType();
            
            bool isRoomSettingsFound =
                _dungeonGenerator.TryGetRoomSettings(roomType, out RoomSettingsMeta roomSettings);

            if (!isRoomSettingsFound)
                return;

            if (!GetRoomRandomDoorway(roomSettings, out DoorwaySettings doorwaySettings))
                return;

            Vector3 lastRoomDoorwayPosition = lastRoomZone.transform.position;
            lastRoomDoorwayPosition.x += lastRoomDoorwaySettings.LocalPosition.x;
            lastRoomDoorwayPosition.z += lastRoomDoorwaySettings.LocalPosition.z;

            Vector3 lastRoomForward = lastRoomZone.transform.forward;
            Debug.Log("Forward: " + lastRoomForward);

            (Vector3 positionOffset, Quaternion spawnRotation) roomOffset = GetRoomOffset(lastRoomForward,
                lastRoomDoorwaySettings.Direction, doorwaySettings);

            Vector3 spawnPosition = lastRoomDoorwayPosition;
            spawnPosition += roomOffset.positionOffset;

            _dungeonGenerator.CreateRoomZone(roomSettings, spawnPosition, roomOffset.spawnRotation);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool GetRoomRandomDoorway(RoomSettingsMeta roomSettings,
            out DoorwaySettings lastRoomDoorwaySettings)
        {
            int lastRoomIndex = _dungeonGenerator.LastRoomIndex;
            lastRoomDoorwaySettings = null;
            
            if (_busyRoomsDoorways.ContainsKey(lastRoomIndex))
            {
                IReadOnlyList<DoorwaySettings> allDoorwaysSettings = roomSettings.DoorwaysSettings.GetAllSettings();

                _availableDoorways.Clear();

                foreach (DoorwaySettings settings in allDoorwaysSettings)
                {
                    DoorwayDirection direction = settings.Direction;
                    bool contains = _busyRoomsDoorways[lastRoomIndex].Contains(direction);

                    if (contains)
                        continue;

                    _availableDoorways.Add(direction);
                    
                    string log1 = Log.HandleLog($"Added direction <gb>{direction}</gb>");
                    Debug.Log(log1);
                }

                int availableDoorwaysAmount = _availableDoorways.Count;

                if (availableDoorwaysAmount == 0)
                    return false;

                int randomIndex = Random.Range(0, availableDoorwaysAmount);
                DoorwayDirection doorway = _availableDoorways[randomIndex];

                bool isLastRoomDoorwaySettingsFound = roomSettings.TryGetDoorwaySettings(doorway,
                    out lastRoomDoorwaySettings);

                if (!isLastRoomDoorwaySettingsFound)
                    return false;

                string log2 = Log.HandleLog($"Random direction <gb>{doorway}</gb>");
                Debug.Log(log2);

                _busyRoomsDoorways[lastRoomIndex].Add(doorway);
            }
            else
            {
                _busyRoomsDoorways.Add(lastRoomIndex, new List<DoorwayDirection>());

                bool isLastRoomDoorwaySettingsFound =
                    roomSettings.TryGetRandomDoorwaySettings(out lastRoomDoorwaySettings);

                if (!isLastRoomDoorwaySettingsFound)
                    return false;
                
                string log2 = Log.HandleLog($"Random doorway! <gb>{lastRoomDoorwaySettings.Direction}</gb>");
                Debug.Log(log2);
            }

            string log = Log.HandleLog($"Selected <gb>{lastRoomDoorwaySettings.Direction}</gb> doorway");
            Debug.Log(log);
            
            return true;
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

        private static DungeonRoomType GetRandomHallwayType()
        {
            List<DungeonRoomType> roomTypes = new()
            {
                DungeonRoomType.Corridor1,
                DungeonRoomType.Corridor2,
                DungeonRoomType.Corridor3,
            };

            int randomIndex = Random.Range(0, roomTypes.Count);
            return roomTypes[randomIndex];
        }
        
        private static (Vector3 positionOffset, Quaternion spawnRotation) GetRoomOffset(Vector3 lastRoomForward,
            DoorwayDirection lastRoomDoorwayDirection, DoorwaySettings doorwaySettings)
        {
            DoorwayDirection startDirection = lastRoomDoorwayDirection;
            
            DoorwayDirection doorwayDirection = doorwaySettings.Direction;
            Vector3 doorwayLocalPosition = doorwaySettings.LocalPosition;
            
            Vector3 positionOffset = Vector3.zero;
            Quaternion rotationOffset = Quaternion.identity;

            switch (lastRoomDoorwayDirection)
            {
                case DoorwayDirection.North:
                    if (lastRoomForward == Vector3.right)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.East;
                    }
                    else if (lastRoomForward == Vector3.left)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.West;
                    }
                    else if (lastRoomForward == Vector3.back)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.South;
                    }
                    break;
                
                case DoorwayDirection.South:
                    if (lastRoomForward == Vector3.right)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.West;
                    }
                    else if (lastRoomForward == Vector3.left)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.East;
                    }
                    else if (lastRoomForward == Vector3.back)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.North;
                    }
                    break;
                
                case DoorwayDirection.West:
                    if (lastRoomForward == Vector3.right)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.North;
                    }
                    else if (lastRoomForward == Vector3.left)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.South;
                    }
                    else if (lastRoomForward == Vector3.back)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.East;
                    }
                    break;
                
                case DoorwayDirection.East:
                    if (lastRoomForward == Vector3.right)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.South;
                    }
                    else if (lastRoomForward == Vector3.left)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.North;
                    }
                    else if (lastRoomForward == Vector3.back)
                    {
                        lastRoomDoorwayDirection = DoorwayDirection.West;
                    }
                    break;
            }

            if (startDirection != lastRoomDoorwayDirection)
            {
                string log = Log.HandleLog($"Direction <gb>{startDirection}</gb> ---> <gb>{lastRoomDoorwayDirection}</gb>");
                Debug.Log(log);
            }

            switch (lastRoomDoorwayDirection)
            {
                case DoorwayDirection.North:
                    switch (doorwayDirection)
                    {
                        case DoorwayDirection.North:
                            PatternFour();
                            break;

                        case DoorwayDirection.South:
                            PatternOne();
                            break;

                        case DoorwayDirection.West:
                            PatternThree();
                            break;

                        case DoorwayDirection.East:
                            PatternTwo();
                            break;
                    }

                    break;

                case DoorwayDirection.South:
                    switch (doorwayDirection)
                    {
                        case DoorwayDirection.North:
                            PatternOne();
                            break;

                        case DoorwayDirection.South:
                            PatternFour();
                            break;

                        case DoorwayDirection.West:
                            PatternTwo();
                            break;

                        case DoorwayDirection.East:
                            PatternThree();
                            break;
                    }

                    break;

                case DoorwayDirection.West:
                    switch (doorwayDirection)
                    {
                        case DoorwayDirection.North:
                            PatternTwo();
                            break;

                        case DoorwayDirection.South:
                            PatternThree();
                            break;

                        case DoorwayDirection.West:
                            PatternFour();
                            break;

                        case DoorwayDirection.East:
                            PatternOne();
                            break;
                    }

                    break;

                case DoorwayDirection.East:
                    switch (doorwayDirection)
                    {
                        case DoorwayDirection.North:
                            PatternThree();
                            break;

                        case DoorwayDirection.South:
                            PatternTwo();
                            break;

                        case DoorwayDirection.West:
                            PatternOne();
                            break;

                        case DoorwayDirection.East:
                            PatternFour();
                            break;
                    }

                    break;
            }

            return (positionOffset, rotationOffset);

            // LOCAL METHODS: -----------------------------

            void PatternOne()
            {
                string log = Log.HandleLog("Pattern <ob>#1</ob>");
                Debug.Log(log);
                
                positionOffset.x -= doorwayLocalPosition.x;
                positionOffset.z -= doorwayLocalPosition.z;
            }

            void PatternTwo()
            {
                string log = Log.HandleLog("Pattern <ob>#2</ob>");
                Debug.Log(log);
                
                rotationOffset = Quaternion.Euler(x: 0, y: 90f, z: 0);
                positionOffset.x -= doorwayLocalPosition.z;
                positionOffset.z += doorwayLocalPosition.x;
            }

            void PatternThree()
            {
                string log = Log.HandleLog("Pattern <ob>#3</ob>");
                Debug.Log(log);
                
                rotationOffset = Quaternion.Euler(x: 0, y: -90f, z: 0);
                positionOffset.x += doorwayLocalPosition.z;
                positionOffset.z -= doorwayLocalPosition.x;
            }

            void PatternFour()
            {
                string log = Log.HandleLog("Pattern <ob>#4</ob>");
                Debug.Log(log);
                
                rotationOffset = Quaternion.Euler(x: 0, y: -180f, z: 0);
                positionOffset.x += doorwayLocalPosition.x;
                positionOffset.z += doorwayLocalPosition.z;
            }
        }
    }
}