using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Player;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Managers.Cameras;
using GameCore.Gameplay.Systems.Health;
using GameCore.Gameplay.Systems.Inventory;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class DeathState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(PlayerEntity playerEntity, ICamerasManager camerasManager)
        {
            _playerEntity = playerEntity;
            _playerReferences = playerEntity.References;
            _camerasManager = camerasManager;
            _deathCameraTimerRoutine = new CoroutineHelper(playerEntity);

            _deathCameraTimerRoutine.GetRoutineEvent += DeathCameraTimerCO;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerEntity _playerEntity;
        private readonly PlayerReferences _playerReferences;
        private readonly ICamerasManager _camerasManager;
        private readonly CoroutineHelper _deathCameraTimerRoutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SetZeroHealth();
            DropAllItems();
            DisableMovement();
            EnableRagdoll();
            EnableHiddenMeshes();
            ToggleDead();
            _deathCameraTimerRoutine.Start();
        }

        public void Exit()
        {
            DisableHiddenMeshes();
            _deathCameraTimerRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetZeroHealth()
        {
            HealthSystem healthSystem = _playerReferences.HealthSystem;
            healthSystem.Kill();
        }

        private void DropAllItems()
        {
            PlayerInventory inventory = _playerEntity.GetInventory();
            inventory.DropAllItems();
        }

        private void DisableMovement()
        {
            PlayerMovementController movementController = _playerReferences.PlayerMovementController;
            movementController.ToggleActiveState(isEnabled: false);
        }

        private void EnableRagdoll() =>
            _playerEntity.ToggleRagdollRpc(enable: true);

        private void EnableHiddenMeshes()
        {
            foreach (GameObject activeObject in _playerReferences.LocalPlayerActiveObjects)
                activeObject.SetActive(false);

            foreach (GameObject inactiveObject in _playerReferences.LocalPlayerInactiveObjects)
                inactiveObject.SetActive(true);
            
            foreach (SkinnedMeshRenderer meshRenderer in _playerReferences.HiddenMeshes)
                meshRenderer.shadowCastingMode = ShadowCastingMode.On;
        }

        private void DisableHiddenMeshes()
        {
            foreach (GameObject activeObject in _playerReferences.LocalPlayerActiveObjects)
                activeObject.SetActive(true);

            foreach (GameObject inactiveObject in _playerReferences.LocalPlayerInactiveObjects)
                inactiveObject.SetActive(false);

            foreach (SkinnedMeshRenderer meshRenderer in _playerReferences.HiddenMeshes)
                meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
        
        private void ToggleDead() =>
            _playerEntity.ToggleDead(isDead: true);

        private void SetCameraStatus(CameraStatus cameraStatus) =>
            _camerasManager.SetCameraStatus(cameraStatus);

        private IEnumerator DeathCameraTimerCO()
        {
            SetCameraStatus(CameraStatus.DeathCamera);

            PlayerConfigMeta playerConfig = _playerEntity.GetConfig();
            float delay = playerConfig.DeathCameraDuration;
            
            yield return new WaitForSeconds(delay);
            
            SetCameraStatus(CameraStatus.Spectator);
        }
    }
}