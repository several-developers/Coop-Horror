#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using GameCore.Gameplay.Events;
using GameCore.InfrastructureTools.Data;
using GameCore.Utilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CustomEditors
{
    public class DataEditor : OdinMenuEditorWindow
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const string EditorMenuItem = EditorConstants.GameMenuName + "/💾 Data Editor";

        private static DataManager _dataManager;
        private static Dictionary<Type, DataBase> _allData;
        private static bool _isShown;
        private static bool _isVisible;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void SetDataManager(DataManager dataManager)
        {
            _dataManager = dataManager;
            _allData ??= new Dictionary<Type, DataBase>();
            
            if (EditorEvents.IsDataEventEmpty())
                EditorEvents.OnDataChangedEvent.AddListener(OnDataChanged);
            
            ClearDataDictionary();
            SetupDataDictionary();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(supportsMultiSelect: true);
            _allData ??= new Dictionary<Type, DataBase>();

            if (_dataManager == null)
            {
                _dataManager = new DataManager();
                _dataManager.LoadLocalData();
            }

            SetupMenuStyle(tree);
            SetupDataDictionary();
            CreateDataTabs(tree);

            tree.SortMenuItemsByName();
            tree.EnumerateTree().SortMenuItemsByName();
            tree.EnumerateTree().AddThumbnailIcons();

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            bool isMenuNotSelected = MenuTree?.Selection == null;

            if (isMenuNotSelected)
                return;

            OdinMenuItem selected = MenuTree.Selection.FirstOrDefault();
            const int toolbarHeight = 23;
            bool selectedIsNull = selected == null;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);

            // Draw label
            GUILayout.Label(selectedIsNull ? " " : selected.Name);

            DrawSaveButton();
            DrawLoadButton();
            DrawDeleteButton();

            SirenixEditorGUI.EndHorizontalToolbar();

            // LOCAL METHODS: -----------------------------

            void DrawSaveButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Save"))
                    return;

                _dataManager.SaveLocalData();
            }

            void DrawLoadButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Load"))
                    return;

                _dataManager.LoadLocalData();
            }

            void DrawDeleteButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Delete"))
                    return;

                _dataManager.DeleteLocalData();
                _dataManager.SaveLocalData();
                _dataManager.LoadLocalData();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _isShown = false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [MenuItem(EditorMenuItem)]
        private static void OpenEditorWindow()
        {
            var window = GetWindow<DataEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(width: 800, height: 600);
            _isShown = true;
        }

        private static void SetupMenuStyle(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = false;
            tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
            tree.DefaultMenuStyle.Height = 28;
            tree.DefaultMenuStyle.IndentAmount = 12;
            tree.DefaultMenuStyle.IconSize = 26;
            tree.DefaultMenuStyle.NotSelectedIconAlpha = 1;
            tree.DefaultMenuStyle.IconPadding = 4;
            tree.DefaultMenuStyle.SelectedColorDarkSkin = EditorDatabase.SelectedColor;
            tree.DefaultMenuStyle.SelectedInactiveColorDarkSkin = EditorDatabase.SelectedInactiveColor;

            //tree.Add("Menu Style", tree.DefaultMenuStyle);
        }

        private static void ClearDataDictionary() =>
            _allData?.Clear();

        private static void SetupDataDictionary()
        {
            IEnumerable<DataBase> allData = _dataManager.GetAllData();

            foreach (DataBase data in allData)
            {
                Type type = data.GetType();
                _allData.TryAdd(type, data);
            }
        }

        private static void CreateDataTabs(OdinMenuTree tree)
        {
            foreach (DataBase data in _allData.Values)
            {
                Type type = data.GetType();
                string dataType = type.Name;
                bool isKeyEmpty = string.IsNullOrEmpty(dataType);

                if (isKeyEmpty)
                    continue;

                string className = dataType.GetNiceName();
                tree.Add(className, data);
            }
        }

        private void OnBecameVisible() =>
            _isVisible = true;

        private void OnBecameInvisible() =>
            _isVisible = false;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private static void OnDataChanged()
        {
            if (!_isShown)
                return;

            if (!_isVisible)
                return;
            
            var window = GetWindow<DataEditor>();

            if (window == null)
                return;
            
            window.BuildMenuTree();
        }
    }
}
#endif