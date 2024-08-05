using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class SittingCameraController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachineVirtualCamera  _camera;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ToggleActiveState(bool isEnabled)
        {
            _camera.gameObject.SetActive(isEnabled);

            if (!isEnabled)
                return;

            var cinemachinePov = _camera.GetCinemachineComponent<CinemachinePOV>();
            cinemachinePov.m_VerticalAxis.Value = 0f;
            cinemachinePov.m_HorizontalAxis.Value = 0f;
        }

        public float GetVerticalAxis()
        {
            var cinemachinePov = _camera.GetCinemachineComponent<CinemachinePOV>();
            return cinemachinePov.m_VerticalAxis.Value;
        }
        
        public float GetHorizontalAxis()
        {
            var cinemachinePov = _camera.GetCinemachineComponent<CinemachinePOV>();
            return cinemachinePov.m_HorizontalAxis.Value;
        }
    }
}