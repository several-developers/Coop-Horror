using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Interactable
{
    public class PlayerTriggerDrawer : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawSphere = true;

        [Title(Constants.References)]
        [SerializeField, Required]
        private SphereCollider _sphereCollider;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawSphere => _drawSphere;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float GetColliderRadius() =>
            _sphereCollider.radius;
    }
}