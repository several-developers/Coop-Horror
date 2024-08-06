using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.VisualManagement;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class MetroManager : IMetroManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MetroManager(IVisualManager visualManager, ILevelProvider levelProvider)
        {
            _visualManager = visualManager;
            _levelProvider = levelProvider;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IVisualManager _visualManager;
        private readonly ILevelProvider _levelProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void TeleportLocalPlayer(bool targetAtSurface)
        {
            PlayerEntity playerEntity = PlayerEntity.GetLocalPlayer();

            bool isMetroDoorFound = _levelProvider.TryGetMetroDoor(!targetAtSurface, out MetroDoor metroDoor);

            if (!isMetroDoorFound)
                return;

            Transform teleportPoint = metroDoor.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;

            playerEntity.References.Rigidbody.velocity = Vector3.zero;
            playerEntity.Teleport(position, rotation);

            ChangePlayerLocation(playerEntity, targetAtSurface, out EntityLocation playerLocation);
            ChangeVisualPreset(playerLocation);
        }

        public void TeleportEntity(ITeleportableEntity entity, bool targetAtSurface)
        {
            bool isMetroDoorFound = _levelProvider.TryGetMetroDoor(!targetAtSurface, out MetroDoor metroDoor);

            if (!isMetroDoorFound)
                return;

            Transform teleportPoint = metroDoor.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;
            
            EntityLocation entityLocation = targetAtSurface ? EntityLocation.MetroPlatform : EntityLocation.Surface;

            //entity.Rigidbody.velocity = Vector3.zero;
            entity.SetEntityLocation(entityLocation);
            entity.SetFloor(Floor.Surface);
            entity.Teleport(position, rotation);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void ChangePlayerLocation(PlayerEntity playerEntity, bool placedAtSurface,
            out EntityLocation entityLocation)
        {
            entityLocation = placedAtSurface ? EntityLocation.MetroPlatform : EntityLocation.Surface;
            playerEntity.SetEntityLocation(entityLocation);
            playerEntity.SetFloor(Floor.Surface);
        }

        private void ChangeVisualPreset(EntityLocation entityLocation)
        {
            VisualPresetType visualPresetType = entityLocation switch
            {
                EntityLocation.MetroPlatform => VisualPresetType.Metro,
                _ => VisualPresetType.DefaultLocation
            };

            _visualManager.ChangePreset(visualPresetType, instant: true);
        }
    }
}