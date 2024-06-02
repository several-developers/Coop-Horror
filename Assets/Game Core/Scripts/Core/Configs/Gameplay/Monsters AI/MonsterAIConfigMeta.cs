using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public abstract class MonsterAIConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: BaseSettings)]
        [BoxGroup(BaseSettingsGroup, showLabel: false), SerializeField]
        private bool _isDamageable = true;

        [BoxGroup(BaseSettingsGroup), SerializeField, Min(0f)]
        [ShowIf(nameof(_isDamageable))]
        private float _health = 100f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsDamageable => _isDamageable;
        public float Health => _health;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string BaseSettings = "Base Settings";
        private const string BaseSettingsGroup = BaseSettings + "/In";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.MonstersAICategory;
    }
}