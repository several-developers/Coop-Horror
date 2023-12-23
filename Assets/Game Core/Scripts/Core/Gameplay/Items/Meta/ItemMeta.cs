using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public abstract class ItemMeta : EditorMeta
    {
        [Title("Item Settings")]
        [SerializeField]
        private string _itemName = "item_name";

        [SerializeField, Min(0), EnableIf(nameof(_canEditItemID))]
        private int _itemID;

        private bool _canEditItemID;

        public override string GetMetaCategory() =>
            EditorConstants.ItemsCategory;

#if UNITY_EDITOR
        [OnInspectorInit]
        private void ResetEditItemIDState() =>
            _canEditItemID = false;

        [Button(25), DisableInPlayMode, HideIf(nameof(_canEditItemID))]
        [GUIColor(0.7f, 0.2f, 0.2f)]
        private void EditItemID() =>
            _canEditItemID = true;

        [Button(25), DisableInPlayMode, ShowIf(nameof(_canEditItemID))]
        [GUIColor(0.2f, 1f, 0.2f)]
        private void StopEditItemID() =>
            _canEditItemID = false;
#endif
    }
}
