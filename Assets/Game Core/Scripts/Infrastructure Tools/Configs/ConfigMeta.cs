using CustomEditors;

namespace GameCore.InfrastructureTools.Configs
{
    public abstract class ConfigMeta : EditorMeta
    {
        // FIELDS: --------------------------------------------------------------------------------

        protected const string Seconds = "seconds";
        protected const string Meters = "meters";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract ConfigScope GetConfigScope();
    }
}