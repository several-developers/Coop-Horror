using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameCore.Gameplay.Network
{
    public class LoadScenesAdd : MonoBehaviour
    {
        public AssetReference sceneRef;


        private void Start()
        {
            StartCoroutine(SetupAddressableSceneLoading());
        }

        /// <summary>
        /// Should be executed immediately after starting the NetworkManager as Server, Host or Client.
        /// </summary>
        public IEnumerator SetupAddressableSceneLoading()
        {
            var sceneManager = NetworkManager.Singleton.SceneManager;
            sceneManager.SceneManagerHandler = new AddressablesSceneManagerHandler();

            var operationHandle = Addressables.LoadResourceLocationsAsync(sceneRef);

            yield return operationHandle;

            if (operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                sceneManager.RegisterExternalScenes(
                    operationHandle.Result.Select(location => location.InternalId).ToArray());
            }

            Addressables.Release(operationHandle);
        }
    }
}