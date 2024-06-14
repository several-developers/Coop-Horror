using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public class ItemPreviewObject : MonoBehaviour
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Show() => SetActive(show: true);

        public void Hide() => SetActive(show: false);

        public void Drop() => Destroy(gameObject);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetActive(bool show) =>
            gameObject.SetActive(show);
    }
}