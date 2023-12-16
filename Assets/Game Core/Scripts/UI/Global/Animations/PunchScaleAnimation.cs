using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Global.Animations
{
    [Serializable]
    public class PunchScaleAnimation
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private Vector3 _scale;

        [SerializeField, Min(0)]
        private float _scaleDuration;

        [SerializeField, Required]
        private RectTransform _scaleRT;

        // FIELDS: --------------------------------------------------------------------------------

        private GameObject _owner;
        private Tweener _scaleTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetOwner(GameObject owner) =>
            _owner = owner;

        public void PlayAnimation()
        {
            _scaleTN.Complete();

            _scaleTN = _scaleRT
                .DOPunchScale(_scale, _scaleDuration, vibrato: 1)
                .SetLink(_owner);
        }
    }
}