using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class ShakeAnimation : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _duration = 1f;

        [SerializeField, Min(0)]
        private int _vibrato = 10;

        [SerializeField]
        private Vector3 _strength;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _target;

        // FIELDS: --------------------------------------------------------------------------------

        private Tweener _shakeTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlayAnimation()
        {
            _shakeTN.Kill(complete: true);

            _shakeTN = _target
                .DOShakeScale(_duration, _strength, _vibrato)
                .SetLink(gameObject);
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugPlayAnimation() => PlayAnimation();
    }
}