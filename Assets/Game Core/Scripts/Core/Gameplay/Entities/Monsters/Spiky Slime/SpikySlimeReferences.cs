using System;
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
        private SkinnedMeshSine _skinnedMeshSine;

        [SerializeField, Required]
        private SkinnedMeshRenderer _slimeRenderer;

        [SerializeField, Required]
        private TextMeshPro _infoTMP;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SkinnedMeshSine SkinnedMeshSine => _skinnedMeshSine;
        public SkinnedMeshRenderer SlimeRenderer => _slimeRenderer;
        public TextMeshPro InfoTMP => _infoTMP;
    }
}