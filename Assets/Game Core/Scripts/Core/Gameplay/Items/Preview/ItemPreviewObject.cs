using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public class ItemPreviewObject : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const string CameraItemLayer = "CameraItem";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ChangeLayer()
        {
            Transform child = transform.GetChild(0);
            Transform[] transforms = child.GetComponentsInChildren<Transform>();

            foreach (Transform target in transforms)
                target.gameObject.layer = LayerMask.NameToLayer(CameraItemLayer);
        }

        public void Show() => SetActive(show: true);

        public void Hide() => SetActive(show: false);

        public void Drop() => Destroy(gameObject);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetActive(bool show) =>
            gameObject.SetActive(show);
    }
}