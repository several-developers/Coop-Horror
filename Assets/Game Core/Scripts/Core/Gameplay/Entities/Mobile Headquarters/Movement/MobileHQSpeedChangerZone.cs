using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [RequireComponent(typeof(SphereCollider))]
    public class MobileHQSpeedChangerZone : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Range(-1f, 2f), SuffixLabel("%", overlay: true)]
        private float _speedPercent;
        
        [SerializeField]
        private bool _drawSphere = true;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private SphereCollider _sphereCollider;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float SpeedPercent => _speedPercent;
        public bool DrawSphere => _drawSphere;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void OnTriggerEnter(Collider other) => HandleTriggerEnter(other);
        
        private void OnTriggerExit(Collider other) => HandleTriggerExit(other);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float GetRadius() =>
            _sphereCollider == null ? 0f : _sphereCollider.radius;
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleTriggerEnter(Collider other)
        {
            if (!IsTrueServer())
                return;
            
            if (!IsMobileEntity(other, out MobileHeadquartersEntity mobileHeadquartersEntity))
                return;
            
            MobileHeadquartersReferences mobileHeadquartersReferences = mobileHeadquartersEntity.References;
            MoveSpeedController moveSpeedController = mobileHeadquartersReferences.MoveSpeedController;
            moveSpeedController.IncreaseSpeedMultiplier(_speedPercent);
        }
        
        private void HandleTriggerExit(Collider other)
        {
            if (!IsTrueServer())
                return;
            
            if (!IsMobileEntity(other, out MobileHeadquartersEntity mobileHeadquartersEntity))
                return;
            
            MobileHeadquartersReferences mobileHeadquartersReferences = mobileHeadquartersEntity.References;
            MoveSpeedController moveSpeedController = mobileHeadquartersReferences.MoveSpeedController;
            moveSpeedController.DecreaseSpeedMultiplier(_speedPercent);
        }

        private static bool IsMobileEntity(Collider other, out MobileHeadquartersEntity mobileHeadquartersEntity) =>
            other.TryGetComponent(out mobileHeadquartersEntity);

        private static bool IsTrueServer() =>
            NetworkHorror.IsTrueServer;
    }
}