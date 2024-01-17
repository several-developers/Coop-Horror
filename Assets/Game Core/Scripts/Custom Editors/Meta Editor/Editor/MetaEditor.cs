#if UNITY_EDITOR
using System.IO;
using System.Linq;
using GameCore.Gameplay.Items;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CustomEditors
{
    // ------------------------------------------------------------------------------------------
    //      Be sure to check out OdinMenuStyleExample.cs as well.
    //      It shows you various ways to customize the look and behaviour of OdinMenuTrees.
    // ------------------------------------------------------------------------------------------ 

    public class MetaEditor : OdinMenuEditorWindow
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const string EditorMenuItem = "🕹 Horror/⚙ Meta Editor";
        private const string GameDataPath = "Assets/Resources/Game Data/";

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(supportsMultiSelect: true);

            SetupMenuStyle(tree);
            LoadAllEditorMeta(tree);
            AddIconsToItems(tree);
            AddDraggableItems(tree);

            tree.EnumerateTree().SortMenuItemsByName();
            tree.EnumerateTree().AddThumbnailIcons();

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            bool isMenuNotSelected = MenuTree?.Selection == null;

            if (isMenuNotSelected)
                return;

            OdinMenuTreeSelection selection = MenuTree.Selection;
            OdinMenuItem selected = MenuTree.Selection.FirstOrDefault();

            var selectedMeta = selection.SelectedValue as EditorMeta;
            int toolbarHeight = MenuTree.Config.SearchToolbarHeight;
            bool selectedIsNull = selected == null;
            bool drawMetaMenuButtons = !selectedIsNull && selectedMeta;

            string metaPath = GameDataPath;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);

            // Draw label
            GUILayout.Label(selectedIsNull ? " " : selected.Name);

            // Create and Selects the newly created item in the editor
            DrawCreateButton();

            if (drawMetaMenuButtons)
            {
                DrawDuplicateButton();
                DrawSaveButton();
                DrawDeleteButton();
            }

            SirenixEditorGUI.EndHorizontalToolbar();

            // LOCAL METHODS: -----------------------------

            void DrawCreateButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Create"))
                    return;

                if (selectedMeta != null)
                {
                    metaPath = AssetDatabase.GetAssetPath(selectedMeta);
                    string fileName = Path.GetFileName(metaPath);
                    metaPath = metaPath.Replace(fileName, "");
                }

                ScriptableObjectCreator.ShowDialog<EditorMeta>(metaPath, TrySelectMenuItemWithObject);
            }

            void DrawDuplicateButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Duplicate"))
                    return;

                metaPath = AssetDatabase.GetAssetPath(selectedMeta);
                ScriptableObjectCreator.Duplicate(selectedMeta, metaPath, ScriptableObjectUtility.SaveAsset);
            }

            void DrawSaveButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Save"))
                    return;

                metaPath = AssetDatabase.GetAssetPath(selectedMeta);
                string newName = selectedMeta.GetMetaName();

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                AssetDatabase.RenameAsset(metaPath, newName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                TrySelectMenuItemWithObject(selectedMeta);
            }

            void DrawDeleteButton()
            {
                if (!SirenixEditorGUI.ToolbarButton("Delete"))
                    return;

                metaPath = AssetDatabase.GetAssetPath(selectedMeta);

                AssetDatabase.DeleteAsset(metaPath);
                AssetDatabase.SaveAssets();
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [MenuItem(EditorMenuItem)]
        private static void OpenEditorWindow()
        {
            var window = GetWindow<MetaEditor>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        private static void SetupMenuStyle(OdinMenuTree tree)
        {
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
            tree.DefaultMenuStyle.Height = 28;
            tree.DefaultMenuStyle.IndentAmount = 12;
            tree.DefaultMenuStyle.IconSize = 24;
            tree.DefaultMenuStyle.NotSelectedIconAlpha = 1;
            tree.DefaultMenuStyle.IconPadding = 4;
            tree.DefaultMenuStyle.SelectedColorDarkSkin = EditorDatabase.SelectedColor;
            tree.DefaultMenuStyle.SelectedInactiveColorDarkSkin = EditorDatabase.SelectedInactiveColor;

            //tree.Add("Menu Style", tree.DefaultMenuStyle);
        }

        private static void LoadAllEditorMeta(OdinMenuTree tree)
        {
            tree.AddAllAssetsAtPath(menuPath: "",
                assetFolderPath: GameDataPath,
                typeof(EditorMeta),
                includeSubDirectories: true);
        }

        private static void AddIconsToItems(OdinMenuTree tree)
        {
            tree.EnumerateTree().AddIcons<ItemMeta>(itemMeta => itemMeta.Icon);
        }

        private static void AddDraggableItems(OdinMenuTree tree)
        {
            AddDraggable<ItemMeta>(tree);
        }

        private static void AddDraggable<T>(OdinMenuTree tree) where T : EditorMeta
        {
            tree.EnumerateTree()
                .Where(odinMenuItem => odinMenuItem.Value as T)
                .ForEach(AddDragHandles);
        }

        private static void AddDragHandles(OdinMenuItem menuItem)
        {
            menuItem.OnDrawItem += _ =>
                DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, allowMove: false, allowSwap: false);
        }
    }
}
#endif