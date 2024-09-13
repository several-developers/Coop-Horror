using System;
using GameCore.Gameplay.Systems.Footsteps;
using GameCore.Gameplay.Systems.Ragdoll;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    [Serializable]
    public class MushroomReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private MushroomSporesTrigger _sporesTriggerPrefab;
        
        [SerializeField, Required]
        private PlayerTrigger _playerTrigger;

        [SerializeField, Required]
        private RagdollController _ragdollController;

        [SerializeField, Required]
        private MonsterFootstepsSystem _footstepsSystem;

        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private GameObject _hatSpores;

        [SerializeField, Required]
        private Transform _modelTransform;

        [SerializeField, Required]
        private ParticleSystem _sporesPS;

        [SerializeField, Required]
        private SkinnedMeshRenderer _hat;
        
        [SerializeField, Required]
        private SkinnedMeshRenderer _eyes;
        
        [SerializeField, Required]
        private SkinnedMeshRenderer _mouth;

        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MushroomSporesTrigger SporesTriggerPrefab => _sporesTriggerPrefab;
        public PlayerTrigger PlayerTrigger => _playerTrigger;
        public RagdollController RagdollController => _ragdollController;
        public MonsterFootstepsSystem FootstepsSystem => _footstepsSystem;
        public Animator Animator => _animator;
        public GameObject HatSpores => _hatSpores;
        public Transform ModelTransform => _modelTransform;
        public ParticleSystem SporesPS => _sporesPS;
        public SkinnedMeshRenderer Hat => _hat;
        public SkinnedMeshRenderer Eyes => _eyes;
        public SkinnedMeshRenderer Mouth => _mouth;
        public TextMeshPro StateTMP => _stateTMP;
    }
}