using System.Linq;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        private Vector3 _itemCameraPreviewPosition;
        
        [SerializeField]
        private Vector3 _itemCameraPreviewRotation;
        
        [SerializeField]
        private Vector3 _itemCameraPreviewScale = Vector3.one;
        
        [SerializeField, Space(height: 5)]
        private Vector3 _itemPlayerPreviewPosition;
        
        [SerializeField]
        private Vector3 _itemPlayerPreviewRotation;
        
        [SerializeField]
        private Vector3 _itemPlayerPreviewScale = Vector3.one;

        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemPreviewObject _itemPreviewPrefab;
        
        //[VerticalGroup(RowRight), SerializeField, Range(0.25f, 2f)]
        //private float _iconScale = 1;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public Sprite Icon => _icon;
        public string ItemName => _itemName;
        public int ItemID => _itemID;
        public Vector3 ItemCameraPreviewPosition => _itemCameraPreviewPosition;
        public Vector3 ItemCameraPreviewRotation => _itemCameraPreviewRotation;
        public Vector3 ItemCameraPreviewScale => _itemCameraPreviewScale;
        public Vector3 ItemPlayerPreviewPosition => _itemPlayerPreviewPosition;
        public Vector3 ItemPlayerPreviewRotation => _itemPlayerPreviewRotation;
        public Vector3 ItemPlayerPreviewScale => _itemPlayerPreviewScale;
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
            
            AssetDatabase.SaveAssetIfDirty(obj: this);
        }

        [VerticalGroup(RowRight)]
        [Button(14), DisableInPlayMode, ShowIf(nameof(_canEditItemID))]
        [GUIColor(1f, 1f, 0.2f)]
        private void SetHighestItemID()
        {
            ItemMeta[] itemsMeta = AssetDatabase
                .FindAssets("t:" + typeof(ItemMeta))
                .Select(x => AssetDatabase.LoadAssetAtPath<ItemMeta>(AssetDatabase.GUIDToAssetPath(x)))
                .ToArray();

            int highestItemID = 0;

            foreach (ItemMeta itemMeta in itemsMeta)
                highestItemID = Mathf.Max(a: highestItemID, b: itemMeta.ItemID);

            _itemID = highestItemID + 1;
            SaveItemID();
        }
#endif
    }
}
