using System;
using DG.Tweening;
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
        private bool _changeInteractableState = true;

        [SerializeField]
        private bool _changeCanvasState;

        [SerializeField]
        private bool _ignoreScaleTime;

        [SerializeField]
        private bool _ignoreCanvasGroupFade;

        [SerializeField, Required, ShowIf(nameof(_changeCanvasState))]
        private Canvas _canvas;

        [SerializeField, Min(0)]
        private float _fadeTime = 0.35f;

        [SerializeField, Required]
        private CanvasGroup _targetCG;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsShown { get; private set; }

        protected float FadeTime => _fadeTime;
        protected CanvasGroup TargetCG => _targetCG;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnShowEvent = delegate { };
        public event Action OnHideEvent = delegate { };
        public event Action OnShownEvent = delegate { };
        public event Action OnHiddenEvent = delegate { };
        public event Action OnDestroyEvent = delegate { };

        private const string UIElementsSettings = "UI Elements Settings";

        private Tweener _fadeTN;
        private bool _destroyOnHide;
        private bool _ignoreDestroy;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            OnShowEvent += OnShowMenu;
            OnHideEvent += OnHideMenu;
            OnShownEvent += OnMenuShown;
            OnHiddenEvent += OnMenuHidden;
        }

        protected virtual void OnDestroy()
        {
            OnShowEvent -= OnShowMenu;
            OnHideEvent -= OnHideMenu;
            OnShownEvent -= OnMenuShown;
            OnHiddenEvent -= OnMenuHidden;
        }

        protected virtual void OnEnable()
        {
            if (!_hiddenByAwake)
                return;

            _targetCG.alpha = 0;

            if (_changeInteractableState)
                DisableInteraction();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Show() =>
            VisibilityState(show: true);

        public virtual void Hide() => Hide(ignoreDestroy: false);

        public virtual void Hide(bool ignoreDestroy)
        {
            _ignoreDestroy = ignoreDestroy;
            VisibilityState(show: false);
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void DestroyOnHide() =>
            _destroyOnHide = true;

        protected void EnableInteraction() => SetInteractableState(isInteractable: true);

        protected void DisableInteraction() => SetInteractableState(isInteractable: false);

        protected virtual float GetFadeInDuration() => _fadeTime;
        
        protected virtual float GetFadeOutDuration() => _fadeTime;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void VisibilityState(bool show)
        {
            IsShown = show;

            if (show)
                OnShowEvent.Invoke();
            else
                OnHideEvent.Invoke();

            if (show && _changeCanvasState)
                _canvas.enabled = true;

            float endValue = show ? 1f : 0f;
            float duration = show ? GetFadeInDuration() : GetFadeOutDuration();

            _fadeTN.Kill(complete: true);

            if (_ignoreCanvasGroupFade)
            {
                Complete();
                return;
            }

            _fadeTN = _targetCG
                .DOFade(endValue, duration)
                .SetUpdate(_ignoreScaleTime)
                .SetLink(gameObject)
                .OnComplete(Complete);

            // LOCAL METHODS: -----------------------------

            void Complete()
            {
                if (show)
                {
                    if (_changeInteractableState)
                        EnableInteraction();

                    OnShownEvent.Invoke();
                }
                else
                {
                    if (_changeInteractableState)
                        DisableInteraction();

                    if (_changeCanvasState)
                        _canvas.enabled = false;

                    if (_destroyOnHide && !_ignoreDestroy)
                        DestroySelf();

                    OnHiddenEvent?.Invoke();
                }
            }
        }
        
        private void SetInteractableState(bool isInteractable)
        {
            _targetCG.interactable = isInteractable;
            _targetCG.blocksRaycasts = isInteractable;
        }

        private void DestroySelf()
        {
            OnHiddenEvent = null;
            OnDestroyEvent.Invoke();

            Destroy(gameObject);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected virtual void OnShowMenu()
        {
        }

        protected virtual void OnHideMenu()
        {
        }

        protected virtual void OnMenuShown()
        {
        }

        protected virtual void OnMenuHidden()
        {
        }
    }
}