using System.Collections.Generic;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Network.PrefabsRegistrar;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Network.Utilities
{
    public class GameplayNetworkPrefabsRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(DiContainer diContainer, INetworkPrefabsRegistrar networkPrefabsRegistrar)
        {
            _diContainer = diContainer;
            _networkPrefabsRegistrar = networkPrefabsRegistrar;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private List<GameObject> _prefabs;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<GameObject> _prefabsToRegister = new();

        private DiContainer _diContainer;
        private INetworkPrefabsRegistrar _networkPrefabsRegistrar;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            DynamicPrefabLoadingUtilities.SetDiContainer(_diContainer);
            RegisterPrefabs();
        }

        private void Start() =>
            GameManager.Instance.SendPlayerLoadedServerRpc();

        private void OnDestroy() => RemovePrefabs();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterPrefabs()
        {
            AddLocalListPrefabs();

            foreach (GameObject prefab in _prefabsToRegister)
                RegisterPrefab(prefab);
        }

        private void RegisterPrefab(GameObject prefab) =>
            _networkPrefabsRegistrar.Register(prefab);

        private void AddLocalListPrefabs() =>
            _prefabsToRegister.AddRange(_prefabs);

        private void RemovePrefabs()
        {
            foreach (GameObject prefab in _prefabsToRegister)
                _networkPrefabsRegistrar.Remove(prefab);
        }
    }
}