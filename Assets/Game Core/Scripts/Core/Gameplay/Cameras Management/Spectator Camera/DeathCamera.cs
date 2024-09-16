using Cinemachine;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.CamerasManagement
{
    public class DeathCamera : MonoBehaviour
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
            PlayerReferences playerReferences = playerEntity.GetReferences();
            Transform hips = playerReferences.HipsRigidbody.transform;
            Transform spine = playerReferences.SpineRigidbody.transform;
            
            _cinemachineFreeLook.m_Follow = spine;
            _cinemachineFreeLook.m_LookAt = hips;
        }

        public void ToggleCameraState(bool isEnabled)
        {
            _camera.enabled = isEnabled;
            _cinemachineFreeLook.enabled = isEnabled;
            _cinemachineBrain.enabled = isEnabled;
        }
    }
}