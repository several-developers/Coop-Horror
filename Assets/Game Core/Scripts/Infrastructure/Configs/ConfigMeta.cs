using CustomEditors;

namespace GameCore.Infrastructure.Configs
{
    public abstract class ConfigMeta : EditorMeta
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public abstract ConfigScope GetConfigScope();
    }
}