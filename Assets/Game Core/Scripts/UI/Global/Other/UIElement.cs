using System;
using DG.Tweening;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Global
{
    public class UIElement : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(UIElementsSettings)]
        [SerializeField]
        private bool _hiddenByAwake;
        
        [SerializeField]
        private bool _ignoreScaleTime;

        [SerializeField]
        private bool _changeCanvasState;

        [SerializeField, Required, ShowIf(nameof(_changeCanvasState))]
        private Canvas _canvas;
        
        [SerializeField, Min(0)]
        private float _fadeTime = 0.35f;

        [SerializeField, Required]
        private CanvasGroup _targetCG;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnHideEvent;
        public event Action OnShowEvent;
        
        private const string UIElementsSettings = "UI Elements Settings";
        
        private Tweener _fadeTN;
        private bool _destroyOnHide;
        private bool _ignoreDestroy;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            if (!_hiddenByAwake)
                return;

            _targetCG.alpha = 0;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Show() =>
            VisibilityState(show: true);

        public void Hide() => Hide(ignoreDestroy: false);

        public void Hide(bool ignoreDestroy)
        {
            _ignoreDestroy = ignoreDestroy;
            VisibilityState(show: false);
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void DestroyOnHide() =>
            _destroyOnHide = true;

        protected void DestroySelf()
        {
            OnHideEvent = null;
            Destroy(gameObject);
        }
        
        protected void VisibilityState(bool show)
        {
            if (show && _changeCanvasState)
                _canvas.enabled = true;
            
            _fadeTN.Kill();
            _fadeTN = _targetCG
                .VisibilityState(show, _fadeTime, _ignoreScaleTime)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    if (!show)
                    {
                        OnHideEvent?.Invoke();

                        if (_changeCanvasState)
                            _canvas.enabled = false;

                        if (_destroyOnHide && !_ignoreDestroy)
                            DestroySelf();
                    }
                    else
                    {
                        OnShowEvent?.Invoke();
                    }
                });
        }

        protected void SetInteractableState(bool isInteractable) =>
            _targetCG.blocksRaycasts = isInteractable;

        protected bool IsShown() =>
            _targetCG.alpha > 0;
    }
}