#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameCore.Utilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomEditors
{
    // 
    // With our custom RPG Editor window, this ScriptableObjectCreator is a replacement for the [CreateAssetMenu] attribute Unity provides.
    // 
    // For instance, if we call ScriptableObjectCreator.ShowDialog<Item>(..., ...), it will automatically find all 
    // ScriptableObjects in your project that inherits from Item and prompts the user with a popup where he 
    // can choose the type of item he wants to create. We then also provide the ShowDialog with a default path,
    // to help the user create it in a specific directory.
    // 

    public static class ScriptableObjectCreator
    {
        public static void ShowDialog<T>(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
            where T : ScriptableObject
        {
            var selector = new ScriptableObjectSelector<T>(defaultDestinationPath, onScriptableObjectCreated);

            if (selector.SelectionTree.EnumerateTree().Count() == 1)
            {
                // If there is only one scriptable object to choose from in the selector, then 
                // we'll automatically select it and confirm the selection. 
                selector.SelectionTree.EnumerateTree().First().Select();
                selector.SelectionTree.Selection.ConfirmSelection();
            }
            else
            {
                // Else, we'll open up the selector in a popup and let the user choose.
                selector.ShowInPopup(200);
            }
        }

        public static void Duplicate<T>(T asset, string newPath, Action<T> onScriptableObjectCreated = null)
            where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);
            newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var obj = AssetDatabase.LoadAssetAtPath<T>(newPath);
            onScriptableObjectCreated?.Invoke(obj);
        }

        // Here is the actual ScriptableObjectSelector which inherits from OdinSelector.
        // You can learn more about those in the documentation: http://sirenix.net/odininspector/documentation/sirenix/odininspector/editor/odinselector(t)
        // This one builds a menu-tree of all types that inherit from T, and when the selection is confirmed, it then prompts the user
        // with a dialog to save the newly created scriptable object.

        private class ScriptableObjectSelector<T> : OdinSelector<Type> where T : ScriptableObject
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ScriptableObjectSelector(string defaultDestinationPath, Action<T> onScriptableObjectCreated = null)
            {
                _onScriptableObjectCreated = onScriptableObjectCreated;
                _defaultDestinationPath = defaultDestinationPath;
                SelectionConfirmed += ShowSaveFileDialog;
            }

            // FIELDS: --------------------------------------------------------------------------------

            private readonly Action<T> _onScriptableObjectCreated;
            private readonly string _defaultDestinationPath;

            // PROTECTED METHODS: ---------------------------------------------------------------------

            protected override void BuildSelectionTree(OdinMenuTree tree)
            {
                // IEnumerable<Type> scriptableObjectTypes = AssemblyUtilities
                //     .GetTypes(AssemblyTypeFlags.CustomTypes)
                //     .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)));

                IEnumerable<Type> scriptableObjectTypes = AssemblyUtilities
                    .GetTypes(AssemblyCategory.All)
                    .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)));

                tree.Selection.SupportsMultiSelect = false;
                tree.Config.DrawSearchToolbar = true;
                tree.AddRange(scriptableObjectTypes, x =>
                    {
                        string niceName = x.GetNiceName();
                        niceName = niceName.Replace("Meta", string.Empty);

                        object obj = ScriptableObject.CreateInstance(x);
                        string menuName = (obj as EditorMeta)?.GetMetaCategory();

                        return $"{menuName}/{niceName.GetNiceName()}";
                    })
                    .AddThumbnailIcons();

                tree.SortMenuItemsByName();
            }

            // PRIVATE METHODS: -----------------------------------------------------------------------

            private void ShowSaveFileDialog(IEnumerable<Type> selection)
            {
                var obj = ScriptableObject.CreateInstance(selection.FirstOrDefault()) as T;
                string dest = _defaultDestinationPath.TrimEnd('/');

                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                    AssetDatabase.Refresh();
                }

                //dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + typeof(T).GetNiceName(), "asset");
                string metaName = obj != null ? obj.GetType().Name.GetNiceName() : "New Editor Meta";
                metaName = metaName.Replace(" Meta", string.Empty); // TEMP
                dest = EditorUtility.SaveFilePanel("Save object as", dest, metaName, "asset");

                if (!string.IsNullOrEmpty(dest) &&
                    PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
                {
                    AssetDatabase.CreateAsset(obj, dest);

                    if (obj is EditorMeta editorMeta)
                        editorMeta.SetMetaName(obj.name);

                    AssetDatabase.Refresh();

                    _onScriptableObjectCreated?.Invoke(obj);
                }
                else
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }
    }
}
#endif