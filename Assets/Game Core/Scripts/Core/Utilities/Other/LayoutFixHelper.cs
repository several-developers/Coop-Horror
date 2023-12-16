using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Utilities
{
    public class LayoutFixHelper
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LayoutFixHelper(MonoBehaviour coroutineRunner, LayoutGroup layoutGroup)
        {
            _coroutineRunner = coroutineRunner;
            _layoutGroup = layoutGroup;
        }

        public LayoutFixHelper(MonoBehaviour coroutineRunner, LayoutGroup layoutGroup,
            ContentSizeFitter contentSizeFitter) :
            this(coroutineRunner, layoutGroup)
        {
            _contentSizeFitter = contentSizeFitter;
            _hasSizeFitter = true;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MonoBehaviour _coroutineRunner;
        private readonly LayoutGroup _layoutGroup;
        private readonly ContentSizeFitter _contentSizeFitter;
        private readonly bool _hasSizeFitter;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void FixLayout()
        {
            if (!_coroutineRunner.isActiveAndEnabled)
                return;

            _coroutineRunner.StartCoroutine(LayoutFixCO());
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator LayoutFixCO()
        {
            if (_hasSizeFitter)
                _contentSizeFitter.enabled = true;

            _layoutGroup.enabled = true;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (_hasSizeFitter)
                _contentSizeFitter.enabled = false;

            _layoutGroup.enabled = false;
        }
    }
}