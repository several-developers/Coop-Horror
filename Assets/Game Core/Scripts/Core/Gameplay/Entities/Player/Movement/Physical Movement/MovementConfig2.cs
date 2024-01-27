using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementConfig2 : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private MovementComponentConfig _movementComponentConfig;
        
        [SerializeField, Required]
        private JumpComponentConfig _jumpComponentConfig;
        
        [SerializeField, Required]
        private PhysicMaterial _groundedPhysMaterial;

        [SerializeField, Required]
        private PhysicMaterial _stayPhysMaterial;

        [SerializeField, Required]
        private PhysicMaterial _flyPhysMaterial;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public MovementComponentConfig MovementComponentConfig => _movementComponentConfig;
        public JumpComponentConfig JumpComponentConfig => _jumpComponentConfig;
        public PhysicMaterial GroundedPhysMaterial => _groundedPhysMaterial;
        public PhysicMaterial StayPhysMaterial => _stayPhysMaterial;
        public PhysicMaterial FlyPhysMaterial => _flyPhysMaterial;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}