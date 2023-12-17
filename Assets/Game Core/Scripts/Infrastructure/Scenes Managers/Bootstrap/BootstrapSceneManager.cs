using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.ScenesManagers.BootstrapScene
{
    public class BootstrapSceneManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct()
        {
        }

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            //Debug.Log("Bootstrap scene loaded.");
        }
    }
}