using Unity.Netcode;
using UnityEngine;

namespace GameCore.Utilities
{
    public class Singleton<T> : NetworkBehaviour where T : Component
    {
        // FIELDS: --------------------------------------------------------------------------------

        private static T _instance;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static T Get()
        {
            if (_instance != null)
                return _instance;
            
            var objs = FindObjectsOfType(typeof(T)) as T[];

            if (objs == null)
            {
                CreateGameObjectInstance();
                return _instance;
            }
                
            if (objs.Length > 0)
                _instance = objs[0];

            if (objs.Length > 1)
                Debug.LogError($"There is more than one {typeof(T).Name} in the scene.");

            if (_instance == null)
                CreateGameObjectInstance();

            return _instance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void CreateGameObjectInstance()
        {
            GameObject obj = new();
            obj.name = $"_{typeof(T).Name}";
            _instance = obj.AddComponent<T>();
        }
    }
}