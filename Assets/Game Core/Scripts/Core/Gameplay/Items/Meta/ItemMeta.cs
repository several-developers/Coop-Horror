using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Items
{
    public abstract class ItemMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(ItemSettings)]
        [HorizontalGroup(Row, 57), VerticalGroup(RowLeft)]
        [PreviewField(57, ObjectFieldAlignment.Left), SerializeField, HideLabel, AssetsOnly]
        private Sprite _icon;

        [VerticalGroup(RowRight), SerializeField]
        private string _itemName = "item_name";
        
        [VerticalGroup(RowRight), SerializeField, EnableIf(nameof(_canEditItemID))]
        private int _itemID;

        [Title("Item Preview Settings")]
        [SerializeField]
        private Vector3 _itemPreviewPosition;
        
        [SerializeField]
        private Vector3 _itemPreviewRotation;
        
        [SerializeField]
        private Vector3 _itemPreviewScale = Vector3.zero;

        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemPreviewObject _itemPreviewPrefab;

        //[VerticalGroup(RowRight), SerializeField, Range(0.25f, 2f)]
        //private float _iconScale = 1;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public Sprite Icon => _icon;
        public int ItemID => _itemID;
        public Vector3 ItemPreviewPosition => _itemPreviewPosition;
        public Vector3 ItemPreviewRotation => _itemPreviewRotation;
        public Vector3 ItemPreviewScale => _itemPreviewScale;
        public ItemPreviewObject ItemPreviewPrefab => _itemPreviewPrefab;

        // FIELDS: --------------------------------------------------------------------------------
        
        private const string ItemSettings = "Item Settings";
        private const string Row = ItemSettings + "/Row";
        private const string RowLeft = Row + "/Left";
        private const string RowRight = Row + "/Right";
        
        private bool _canEditItemID;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.ItemsCategory;

        // PRIVATE METHODS: -----------------------------------------------------------------------

#if UNITY_EDITOR
        [OnInspectorInit]
        private void ResetEditItemIDState() =>
            _canEditItemID = false;

        [VerticalGroup(RowRight)]
        [Button(14), DisableInPlayMode, HideIf(nameof(_canEditItemID))]
        [GUIColor(0.7f, 0.2f, 0.2f)]
        private void EditItemID() =>
            _canEditItemID = true;

        [VerticalGroup(RowRight)]
        [Button(14), DisableInPlayMode, ShowIf(nameof(_canEditItemID))]
        [GUIColor(0.2f, 1f, 0.2f)]
        private void SaveItemID()
        {
            _canEditItemID = false;
            
            UnityEditor.AssetDatabase.SaveAssetIfDirty(obj: this);
        }
#endif
    }
}
