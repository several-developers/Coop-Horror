using System.Collections.Generic;
using GameCore.Enums;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    public class RoomZonesGeneratorLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RoomZonesGeneratorLogic(DungeonGenerator dungeonGenerator)
        {
            _dungeonGenerator = dungeonGenerator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DungeonGenerator _dungeonGenerator;

        private Dictionary<int, List<DoorwayDirection>> _busyRoomsDoorways = new();
        private List<RoomZone> _roomZonesInstances = new();
        private List<DoorwayDirection> _availableDoorways = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void HandleRoomSpawn(DungeonRoomType roomType)
        {
            bool isRoomSettingsFound =
                _dungeonGenerator.TryGetRoomSettings(roomType, out RoomSettingsMeta roomSettings);

            if (!isRoomSettingsFound)
                return;

            int lastRoomIndex = _dungeonGenerator.LastRoomIndex;

            RoomZone lastRoomZone = _roomZonesInstances[^1];
            RoomSettingsMeta lastRoomSettings = lastRoomZone.GetRoomSettings();

            if (!GetRoomRandomDoorway(lastRoomSettings, out DoorwaySettings lastRoomDoorwaySettings))
                return;

            if (!GetRoomRandomDoorway(roomSettings, out DoorwaySettings doorwaySettings))
                return;

            Vector3 lastRoomDoorwayPosition = lastRoomZone.transform.position;
            lastRoomDoorwayPosition.x += lastRoomDoorwaySettings.LocalPosition.x;
            lastRoomDoorwayPosition.z += lastRoomDoorwaySettings.LocalPosition.z;

            DoorwayDirection doorwayDirection = doorwaySettings.Direction;
            Vector3 doorwayLocalPosition = doorwaySettings.LocalPosition;

            (Vector3 positionOffset, Quaternion spawnRotation) roomOffset =
                GetRoomOffset(lastRoomDoorwaySettings.Direction, doorwayDirection, doorwayLocalPosition);

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

                _busyRoomsDoorways[lastRoomIndex].Add(doorway);
            }
            else
            {
                _busyRoomsDoorways.Add(lastRoomIndex, new List<DoorwayDirection>());

                bool isLastRoomDoorwaySettingsFound =
                    roomSettings.TryGetRandomDoorwaySettings(out lastRoomDoorwaySettings);

                if (!isLastRoomDoorwaySettingsFound)
                    return false;
            }

            return true;
        }
        
        private static (Vector3 positionOffset, Quaternion spawnRotation) GetRoomOffset(
            DoorwayDirection lastRoomDoorwayDirection,
            DoorwayDirection doorwayDirection, Vector3 doorwayLocalPosition)
        {
            Vector3 positionOffset = Vector3.zero;
            Quaternion rotationOffset = Quaternion.identity;

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
                positionOffset.x -= doorwayLocalPosition.x;
                positionOffset.z -= doorwayLocalPosition.z;
            }

            void PatternTwo()
            {
                rotationOffset = Quaternion.Euler(x: 0, y: 90f, z: 0);
                positionOffset.x -= doorwayLocalPosition.z;
                positionOffset.z += doorwayLocalPosition.x;
            }

            void PatternThree()
            {
                rotationOffset = Quaternion.Euler(x: 0, y: -90f, z: 0);
                positionOffset.x += doorwayLocalPosition.z;
                positionOffset.z -= doorwayLocalPosition.x;
            }

            void PatternFour()
            {
                rotationOffset = Quaternion.Euler(x: 0, y: -180f, z: 0);
                positionOffset.x += doorwayLocalPosition.x;
                positionOffset.z += doorwayLocalPosition.z;
            }
        }
    }
}