using System;
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
        private Animator _animator;

        [SerializeField, Required]
        private GameObject _hatSpores;

        [SerializeField, Required]
        private SkinnedMeshRenderer _hat;
        
        [SerializeField, Required]
        private SkinnedMeshRenderer _eyes;
        
        [SerializeField, Required]
        private SkinnedMeshRenderer _mouth;

        [SerializeField, Required]
        private TextMeshPro _stateTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Animator Animator => _animator;
        public GameObject HatSpores => _hatSpores;
        public SkinnedMeshRenderer Hat => _hat;
        public SkinnedMeshRenderer Eyes => _eyes;
        public SkinnedMeshRenderer Mouth => _mouth;
        public TextMeshPro StateTMP => _stateTMP;
    }
}