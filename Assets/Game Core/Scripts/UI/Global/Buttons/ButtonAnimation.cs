using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI.Global.Buttons
{
    public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _checkInteractable;

        [SerializeField, Required]
        [ShowIf(nameof(_checkInteractable))]
        private Button _button;

        [SerializeField, Min(0)]
        private float _scaleTime = 0.15f;

        [SerializeField]
        private Vector2 _scale = new(0.9f, 0.9f);

        [Title(Constants.References)]
        [InfoBox("Missing 'Scale RT'!", InfoMessageType.Error, "@_scaleRT == null")]
        [SerializeField]
        private RectTransform _scaleRT;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsEnabled { get; set; } = true;

        // FIELDS: --------------------------------------------------------------------------------

        private Tweener _scaleTN;
        private Vector3 _startScale;
        private Vector3 _finalScale;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() =>
            _startScale = _scaleRT.localScale;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnButtonDown()
        {
            if (!IsEnabled)
                return;

            _scaleTN.Complete();
            _finalScale = _startScale * _scale;
            _finalScale.z = _finalScale.x;

            _scaleTN = _scaleRT
                .DOScale(_finalScale, _scaleTime)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void OnButtonUp()
        {
            if (!IsEnabled) return;

            _scaleTN.Complete();

            _scaleTN = _scaleRT
                .DOScale(_startScale, _scaleTime)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_checkInteractable && !_button.interactable)
                return;

            OnButtonDown();
        }

        public void OnPointerUp(PointerEventData eventData) => OnButtonUp();
    }
}