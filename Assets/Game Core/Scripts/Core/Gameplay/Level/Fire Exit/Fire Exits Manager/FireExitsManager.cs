using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.VisualManagement;
using UnityEngine;

namespace GameCore.Gameplay.Level
{
    public class FireExitsManager : IFireExitsManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FireExitsManager(IVisualManager visualManager, ILevelProvider levelProvider)
        {
            _visualManager = visualManager;
            _levelProvider = levelProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IVisualManager _visualManager;
        private readonly ILevelProvider _levelProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TeleportLocalPlayerToFireExit(Floor floor, bool isInStairsLocation)
        {
            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();
            FireExit fireExit;

            // Reversed
            bool isFireExitFound = isInStairsLocation
                ? _levelProvider.TryGetOtherFireExit(floor, out fireExit)
                : _levelProvider.TryGetStairsFireExit(floor, out fireExit);

            if (!isFireExitFound)
                return;

            Transform teleportPoint = fireExit.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;

            playerEntity.References.Rigidbody.velocity = Vector3.zero;
            playerEntity.Teleport(position, rotation);

            ChangePlayerLocation(playerEntity, floor, isInStairsLocation, out EntityLocation playerLocation);
            ChangeVisualPreset(playerLocation);
        }

        public void TeleportEntityToFireExit(ITeleportableEntity entity, Floor floor, bool isInStairsLocation)
        {
            FireExit fireExit;

            // Reversed
            bool isFireExitFound = isInStairsLocation
                ? _levelProvider.TryGetOtherFireExit(floor, out fireExit)
                : _levelProvider.TryGetStairsFireExit(floor, out fireExit);

            if (!isFireExitFound)
                return;

            Transform teleportPoint = fireExit.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;

            //entity.Rigidbody.velocity = Vector3.zero;
            entity.Teleport(position, rotation);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void ChangePlayerLocation(PlayerEntity playerEntity, Floor floor, bool isInStairsLocation,
            out EntityLocation entityLocation)
        {
            entityLocation = EntityLocation.Stairs;

            if (isInStairsLocation)
            {
                entityLocation = floor == Floor.Surface
                    ? EntityLocation.LocationSurface
                    : EntityLocation.Dungeon;
            }

            playerEntity.ChangePlayerLocation(entityLocation);
        }

        private void ChangeVisualPreset(EntityLocation entityLocation)
        {
            VisualPresetType visualPresetType = VisualPresetType.DefaultLocation;
            
            switch (entityLocation)
            {
                case EntityLocation.Dungeon:
                case EntityLocation.Stairs:
                    visualPresetType = VisualPresetType.Dungeon;
                    break;
            }
            
            _visualManager.ChangePreset(visualPresetType, instant: true);
        }
    }
}