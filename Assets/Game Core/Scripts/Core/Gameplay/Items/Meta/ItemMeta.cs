using System;
using System.Linq;
using CustomEditors;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        [Title("Common")]
        [SerializeField, Min(0f)]
        private float _scaleMultiplier = 1f;

        [SerializeField]
        private RigPresetType _rigPresetType = RigPresetType.RightHandBase;

        [SerializeField]
        private ItemHandPlacement _itemHandPlacement = ItemHandPlacement.Right;

        [Title("First Person Preview Settings")]
        [SerializeField]
        private ItemPose _fpsItemPreview;

        [Title("Third Person Preview Settings")]
        [SerializeField]
        private ItemPose _tpsItemPreview;

        //[VerticalGroup(RowRight), SerializeField, Range(0.25f, 2f)]
        //private float _iconScale = 1;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Sprite Icon => _icon;
        public string ItemName => _itemName;
        public float ScaleMultiplier => _scaleMultiplier;
        public RigPresetType RigPresetType => _rigPresetType;
        public ItemHandPlacement ItemHandPlacement => _itemHandPlacement;
        public int ItemID => _itemID;
        public ItemPose FpsItemPreview => _fpsItemPreview;
        public ItemPose TpsItemPreview => _tpsItemPreview;
        public ItemPreviewObject ItemPreviewPrefab => null;

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

        [Serializable]
        public class ItemPose
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private Vector3 _position;

            [SerializeField]
            private Vector3 _eulerRotation;

            [SerializeField]
            private Vector3 _scale = Vector3.one;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector3 Position => _position;
            public Vector3 EulerRotation => _eulerRotation;
            public Vector3 Scale => _scale;

            // PRIVATE METHODS: -----------------------------------------------------------------------

#if UNITY_EDITOR
            [Button(buttonSize: 21), GUIColor(r: 1f, g: 0.125f, b: 0.447f), PropertySpace(3f)]
            [ShowIf(nameof(HasValidSelection))]
            private void CopyTransform()
            {
                Transform[] transforms = Selection.transforms;
                bool isValid = transforms.Length == 1;

                if (!isValid)
                    return;

                Transform transform = transforms[0];

                Vector3 position = transform.localPosition;
                Vector3 rotation = transform.localEulerAngles;
                Vector3 scale = transform.localScale;
                
                float positionX = (float)Math.Round((decimal)position.x, 3);
                float positionY = (float)Math.Round((decimal)position.y, 3);
                float positionZ = (float)Math.Round((decimal)position.z, 3);
                
                float rotationX = Mathf.FloorToInt(rotation.x);
                float rotationY = Mathf.FloorToInt(rotation.y);
                float rotationZ = Mathf.FloorToInt(rotation.z);
                
                float scaleX = (float)Math.Round((decimal)scale.x, 3);
                float scaleY = (float)Math.Round((decimal)scale.y, 3);
                float scaleZ = (float)Math.Round((decimal)scale.z, 3);

                position = new Vector3(positionX, positionY, positionZ);
                rotation = new Vector3(rotationX, rotationY, rotationZ);
                scale = new Vector3(scaleX, scaleY, scaleZ);
                
                _position = position;
                _eulerRotation = rotation;
                _scale = scale;
            }

            private static bool HasValidSelection()
            {
                Transform[] transforms = Selection.transforms;
                bool isValid = transforms.Length == 1;
                return isValid;
            }
#endif
        }
    }
}