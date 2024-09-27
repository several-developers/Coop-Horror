using System;
using DG.Tweening;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Global.Other
{
    [Serializable]
    public class CanvasGroupSwitcher
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Min(0)]
        private float _fadeTime = 0.35f;
        
        [SerializeField, Required]
        private CanvasGroup _targetCG;

        // FIELDS: --------------------------------------------------------------------------------

        private GameObject _owner;
        private Tweener _fadeTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetOwner(GameObject owner) =>
            _owner = owner;
        
        public void Show() =>
            VisibilityState(show: true);

        public void Hide() => VisibilityState(show: false);
        
        public void VisibilityState(bool show)
        {
            _fadeTN.Kill();
            _fadeTN = _targetCG
                .VisibilityState(show, _fadeTime)
                .SetLink(_owner);
        }
    }
}