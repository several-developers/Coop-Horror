using GameCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MetaEditor
{
    public abstract class EditorMeta : ScriptableObject
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(EditorConstants.MetaSettings)]
        [BoxGroup(EditorConstants.MetaSettingsIn, showLabel: false), SerializeField]
        private string _metaName;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnEnable() =>
            _metaName = name;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetMetaName() => _metaName;
        
        public void SetMetaName(string newName) =>
            _metaName = newName;
        
        public virtual string GetMetaCategory() =>
            EditorConstants.NoCategory;
    }
}