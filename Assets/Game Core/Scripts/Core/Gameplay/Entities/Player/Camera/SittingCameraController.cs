using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class SittingCameraController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private GameObject _camera;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ToggleActiveState(bool isEnabled) =>
            _camera.SetActive(isEnabled);
    }
}