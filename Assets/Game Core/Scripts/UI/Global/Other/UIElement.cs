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
        private bool _changeInteractableState;
        
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
        public event Action OnDestroyEvent;
        
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
            
            if (_changeInteractableState)
                DisableInteraction();
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

        protected void VisibilityState(bool show)
        {
            if (show && _changeCanvasState)
                _canvas.enabled = true;
            
            _fadeTN.Kill(complete: true);
            _fadeTN = _targetCG
                .VisibilityState(show, _fadeTime, _ignoreScaleTime)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    if (show)
                    {
                        if (_changeInteractableState)
                            EnableInteraction();
                        
                        OnShowEvent?.Invoke();
                    }
                    else
                    {
                        if (_changeInteractableState)
                            DisableInteraction();
                        
                        if (_changeCanvasState)
                            _canvas.enabled = false;

                        if (_destroyOnHide && !_ignoreDestroy)
                            DestroySelf();
                        
                        OnHideEvent?.Invoke();
                    }
                });
        }

        protected void EnableInteraction() => SetInteractableState(isInteractable: true);

        protected void DisableInteraction() => SetInteractableState(isInteractable: false);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetInteractableState(bool isInteractable) =>
            _targetCG.blocksRaycasts = isInteractable;
        
        private void DestroySelf()
        {
            OnHideEvent = null;
            OnDestroyEvent?.Invoke();
            
            Destroy(gameObject);
        }
    }
}