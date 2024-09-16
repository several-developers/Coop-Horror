using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    [Serializable]
    public class SpikySlimeReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private SpikySlimeAttackTrigger _attackTrigger;
        
        [SerializeField, Required]
        private SkinnedMeshSine _skinnedMeshSine;

        [SerializeField, Required]
        private SkinnedMeshRenderer _slimeRenderer;

        [SerializeField, Required]
        private ShakeAnimation _shakeAnimation;

        [SerializeField, Required]
        private Transform _modelPivot;
        
        [SerializeField, Required]
        private Transform _anchorPoints;
        
        [SerializeField, Required]
        private SphereCollider _slimeTrigger;
        
        [SerializeField, Required]
        private TextMeshPro _infoTMP;

        [SerializeField, Required, Space(height: 5)]
        private List<SpringJoint> _springJoints;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SpikySlimeAttackTrigger AttackTrigger => _attackTrigger;
        public SkinnedMeshSine SkinnedMeshSine => _skinnedMeshSine;
        public SkinnedMeshRenderer SlimeRenderer => _slimeRenderer;
        public ShakeAnimation ShakeAnimation => _shakeAnimation;
        public Transform ModelPivot => _modelPivot;
        public Transform AnchorPoints => _anchorPoints;
        public SphereCollider SlimeTrigger => _slimeTrigger;
        public TextMeshPro InfoTMP => _infoTMP;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public IReadOnlyList<SpringJoint> GetAllSpringJoints() => _springJoints;
    }
}