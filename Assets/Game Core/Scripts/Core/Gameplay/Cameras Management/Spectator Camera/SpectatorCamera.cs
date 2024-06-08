using Cinemachine;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.CamerasManagement
{
    public class SpectatorCamera : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Camera _camera;
        
        [SerializeField, Required]
        private CinemachineFreeLook _cinemachineFreeLook;
        
        [SerializeField, Required]
        private CinemachineBrain _cinemachineBrain;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            transform.SetParent(null);
            transform.SetAsLastSibling();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateTarget(PlayerEntity playerEntity)
        {
            Transform playerTransform = playerEntity.transform;
            Transform spectatorTarget = playerEntity.References.SpectatorCameraTarget;
            
            _cinemachineFreeLook.m_Follow = playerTransform;
            _cinemachineFreeLook.m_LookAt = spectatorTarget;
        }

        public void ToggleCameraState(bool isEnabled)
        {
            _camera.enabled = isEnabled;
            _cinemachineFreeLook.enabled = isEnabled;
            _cinemachineBrain.enabled = isEnabled;
        }
    }
}