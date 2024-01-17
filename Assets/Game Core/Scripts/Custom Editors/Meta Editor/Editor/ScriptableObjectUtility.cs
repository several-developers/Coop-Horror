#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomEditors
{
    public static class ScriptableObjectUtility
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void SaveAsset<T>(T asset) where T : ScriptableObject
        {
            EditorUtility.SetDirty(asset);
                        
            if (GetAssetNameWithoutExtension(asset) != asset.name)
                RenameAsset(asset, asset.name);
                        
            AssetDatabase.SaveAssets();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static void RenameAsset<T>(T asset, string newName) where T : ScriptableObject =>
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), newName);

        private static string GetAssetNameWithoutExtension<T>(T asset) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);

            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
#endif