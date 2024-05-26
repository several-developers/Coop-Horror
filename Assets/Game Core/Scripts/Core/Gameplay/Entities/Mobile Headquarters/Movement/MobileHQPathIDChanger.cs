using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [RequireComponent(typeof(SphereCollider))]
    public class MobileHQPathIDChanger : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private int _pathID;
        
        [SerializeField]
        private bool _drawSphere = true;

        [Title(Constants.References)]
        [SerializeField, Required]
        private SphereCollider _sphereCollider;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public int PathID => _pathID;
        public bool DrawSphere => _drawSphere;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnTriggerEnter(Collider other) => HandleTriggerEnter(other);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public float GetRadius() =>
            _sphereCollider == null ? 0f : _sphereCollider.radius;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleTriggerEnter(Collider other)
        {
            if (!NetworkHorror.IsTrueServer)
                return;
            
            bool isMobileHQ = other.TryGetComponent(out IMobileHeadquartersEntity _);

            if (!isMobileHQ)
                return;

            MobileHeadquartersEntity.LastPathID = _pathID;
        }
    }
}