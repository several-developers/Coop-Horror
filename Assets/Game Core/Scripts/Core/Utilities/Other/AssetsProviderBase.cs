using UnityEngine;

namespace GameCore.Utilities
{
    public abstract class AssetsProviderBase
    {
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected static T Load<T>(string path) where T : Object =>
            Resources.Load<T>(path);

        protected static T[] LoadAll<T>(string path) where T : Object =>
            Resources.LoadAll<T>(path);
    }
}