using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Gameplay.HUD
{
    public class HUDStartAnimation : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Vector2 _offset;

        [SerializeField]
        [Min(0)]
        private float _moveDelay;

        [SerializeField]
        [Min(0)]
        private float _moveTime;

        [SerializeField]
        private Ease _moveEase;

        [Title(Constants.References)]
        [SerializeField, Required]
        private RectTransform _rectTransform;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Start() => StartAnimation();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [Button, DisableInEditorMode]
        private void StartAnimation()
        {
            _rectTransform.DOSizeDelta(_offset, 0, true);
            _rectTransform.DOSizeDelta(Vector2.zero, _moveTime, true)
                .SetDelay(_moveDelay)
                .SetEase(_moveEase)
                .SetLink(gameObject);
        }
    }
}