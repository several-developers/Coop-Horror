using UnityEngine;

namespace EFPController
{

    [DefaultExecutionOrder(-999)]
    public class GameInit
    {

        private const string defaultGamePrefab = "Game";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            GameObject gamePrefab = Resources.Load<GameObject>(defaultGamePrefab);
            if (gamePrefab != null) 
            {
                GameObject.Instantiate(gamePrefab);
            } else {
                Debug.LogError("Cant find " + defaultGamePrefab + " prefab in Resources folders!");
            }
        }

    }

}