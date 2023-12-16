using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Other.MainCamera
{
    public class MainCamera : MonoBehaviour, IMainCamera
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Camera _camera;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Camera GetCamera() => _camera;
    }
}