using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.CameraManagement
{
    public class PlayerCamera : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField]
        private CameraReferences _cameraReferences;

        // PROPERTIES: ----------------------------------------------------------------------------

        public CameraReferences CameraReferences => _cameraReferences;

        // FIELDS: --------------------------------------------------------------------------------
        
        private Transform _transform;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _transform = transform;

        private void Start()
        {
            _transform.SetParent(null);
            _transform.SetAsLastSibling();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ToggleCameraState(bool isEnabled)
        {
            Camera mainCamera = _cameraReferences.MainCamera;
            CinemachineBrain cinemachineBrain = _cameraReferences.CinemachineBrain;

            mainCamera.enabled = isEnabled;
            cinemachineBrain.enabled = isEnabled;
        }
    }
}