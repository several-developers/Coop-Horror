using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Infrastructure.Providers.Global;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    /// <summary>
    /// This class serves as the playground of the dynamic prefab loading use-cases. It integrates API from this sample
    /// to use at post-connection time such as: connection approval for syncing late-joining clients, dynamically
    /// loading a collection of network prefabs on the host and all connected clients, synchronously spawning a
    /// dynamically loaded network prefab across connected clients, and spawning a dynamically loaded network prefab as
    /// network-invisible for all clients until they load the prefab locally (in which case it becomes network-visible
    /// to the client).
    /// </summary>
    /// <remarks>
    /// For more details on the API usage, see the in-project readme (which includes links to further resources,
    /// including the project's technical document).
    /// </remarks>
#warning НАЗВАТЬ Dynamic Prefabs Factory или Network Prefabs Factory
    public class DynamicPrefabsLoader : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator) =>
            _dynamicPrefabsLoaderDecorator = dynamicPrefabsLoaderDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private List<AssetReferenceGameObject> _dynamicPrefabsReferences;

        // FIELDS: --------------------------------------------------------------------------------

        private const float NetworkSpawnTimeoutSeconds = 3000f;
        private const int ArtificialDelayMilliseconds = 1000;

        // A storage where we keep association between prefab (hash of it's GUID)
        // and the spawned network objects that use it.
        private readonly Dictionary<int, HashSet<NetworkObject>> _prefabHashToNetworkObjectID = new();

        private IDynamicPrefabsLoaderDecorator _dynamicPrefabsLoaderDecorator;
        private NetworkManager _networkManager;

        private float _synchronousSpawnTimeoutTimer;
        private int _synchronousSpawnAckCount;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _networkManager = NetworkManager.Singleton;

        protected override void StartAll()
        {
            DynamicPrefabLoadingUtilities.Init(_networkManager);

            // In the use-cases where connection approval is implemented, the server can begin to validate a user's
            // connection payload, and either approve or deny connection to the joining client.
            _networkManager.NetworkConfig.ConnectionApproval = true;

            // Here, we keep ForceSamePrefabs disabled. This will allow us to dynamically add network prefabs to
            // Netcode for GameObject after establishing a connection.
            _networkManager.NetworkConfig.ForceSamePrefabs = false;
        }

        protected override void OnDestroyAll()
        {
            DynamicPrefabLoadingUtilities.UnloadAndReleaseAllDynamicPrefabs();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitOwner()
        {
            _dynamicPrefabsLoaderDecorator.OnTrySpawnGameObjectPrefabEvent += OnTrySpawnGameObjectPrefab;
            _dynamicPrefabsLoaderDecorator.OnTrySpawnNetworkObjectPrefabEvent += OnTrySpawnNetworkObjectPrefab;
        }

        protected override void DespawnOwner()
        {
            _dynamicPrefabsLoaderDecorator.OnTrySpawnGameObjectPrefabEvent -= OnTrySpawnGameObjectPrefab;
            _dynamicPrefabsLoaderDecorator.OnTrySpawnNetworkObjectPrefabEvent -= OnTrySpawnNetworkObjectPrefab;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTaskVoid PreloadPrefabs()
        {
            var tasks = new List<UniTask>();

            foreach (AssetReferenceGameObject asset in _dynamicPrefabsReferences)
            {
                UniTask task = PreloadDynamicPrefabOnServerAndStartLoadingOnAllClients(asset.AssetGUID);
                tasks.Add(task);
            }

            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// This call preloads the dynamic prefab on the server and sends a client rpc to all the clients
        /// to do the same.
        /// </summary>
        /// <param name="guid"></param>
        private async UniTask PreloadDynamicPrefabOnServerAndStartLoadingOnAllClients(string guid)
        {
            if (_networkManager.IsServer)
            {
                var assetGuid = new AddressableGUID
                {
                    Value = guid
                };

                if (DynamicPrefabLoadingUtilities.IsPrefabLoadedOnAllClients(assetGuid))
                {
                    Debug.Log(message: "Prefab is already loaded by all peers");
                    return;
                }

                // Update UI for each client that is requested to load a certain prefab.
                foreach (ulong client in _networkManager.ConnectedClients.Keys)
                {
                    // m_InGameUI.ClientLoadedPrefabStatusChanged(client,
                    //     assetGuid.GetHashCode(),
                    //     "Undefined",
                    //     InGameUI.LoadStatus.Loading);
                }

                Debug.Log(message: "Loading dynamic prefab on the clients...");
                LoadAddressableClientRpc(assetGuid);

                await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid, ArtificialDelayMilliseconds);

                // Server loaded a prefab, update UI with the loaded asset's name.
                DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid,
                    out AsyncOperationHandle<GameObject> loadedGameObject);

                // Every client loaded dynamic prefab, their respective ClientUIs in case they loaded first.
                foreach (ulong client in _networkManager.ConnectedClients.Keys)
                {
                    // m_InGameUI.ClientLoadedPrefabStatusChanged(client,
                    //     assetGuid.GetHashCode(),
                    //     loadedGameObject.Result.name,
                    //     InGameUI.LoadStatus.Loaded);
                }
            }
        }

        /// <summary>
        /// This call attempts to spawn a prefab by it's addressable guid - it ensures that all the clients have
        /// loaded the prefab before spawning it, and if the clients fail to acknowledge that they've loaded a prefab -
        /// the spawn will fail.
        /// </summary>
        private async void TryLoadAndGetDynamicGameObjectPrefab(string guid, Action<GameObject> loadCallback)
        {
            GameObject prefab = await TryLoadAndGetDynamicPrefab(guid, registerAtNetwork: false);

            if (prefab == null)
                SendError();
            else
                SendSuccess();

            // LOCAL METHODS: -----------------------------

            void SendSuccess() =>
                loadCallback?.Invoke(prefab);

            void SendError() =>
                loadCallback?.Invoke(obj: null);
        }

        /// <summary>
        /// This call attempts to spawn a prefab by it's addressable guid - it ensures that all the clients have
        /// loaded the prefab before spawning it, and if the clients fail to acknowledge that they've loaded a prefab -
        /// the spawn will fail.
        /// </summary>
        private async void TryLoadAndGetDynamicNetworkObjectPrefab(string guid, Action<NetworkObject> loadCallback)
        {
            GameObject prefab = await TryLoadAndGetDynamicPrefab(guid, registerAtNetwork: true);

            if (prefab == null)
            {
                SendError();
                return;
            }

            if (prefab.TryGetComponent(out NetworkObject prefabNetworkObject))
                SendSuccess();
            else
                SendError();

            // LOCAL METHODS: -----------------------------

            void SendSuccess() =>
                loadCallback?.Invoke(prefabNetworkObject);

            void SendError() =>
                loadCallback?.Invoke(obj: null);
        }

        private async UniTask<GameObject> TryLoadAndGetDynamicPrefab(string guid, bool registerAtNetwork)
        {
            if (!IsServer)
            {
                SendError();
                return null;
            }

            var assetGuid = new AddressableGUID
            {
                Value = guid
            };

            if (DynamicPrefabLoadingUtilities.IsPrefabLoadedOnAllClients(assetGuid))
            {
                // Debug.Log(message: "Prefab is already loaded by all peers, we can spawn it immediately");

                GameObject prefab = LoadAndSendPrefab(assetGuid);

                if (prefab == null)
                    SendError();

                return prefab;
            }

            _synchronousSpawnAckCount = 0;
            _synchronousSpawnTimeoutTimer = 0;

            Debug.Log(message: "Loading dynamic prefab on the clients...");
            LoadAddressableClientRpc(assetGuid);

            // Server is starting to load a prefab, update UI.
            // m_InGameUI.ClientLoadedPrefabStatusChanged(NetworkManager.ServerClientId, assetGuid.GetHashCode(),
            //     "Undefined", InGameUI.LoadStatus.Loading);

            // Load the prefab on the server, so that any late-joiner will need to load that prefab also.
            await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid, ArtificialDelayMilliseconds,
                registerAtNetwork);

            // Server loaded a prefab, update UI with the loaded asset's name.
            DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid,
                out AsyncOperationHandle<GameObject> loadedGameObject);

            // m_InGameUI.ClientLoadedPrefabStatusChanged(NetworkManager.ServerClientId, assetGuid.GetHashCode(),
            //     loadedGameObject.Result.name, InGameUI.LoadStatus.Loaded);

            int requiredAcknowledgementsCount = IsHost
                ? _networkManager.ConnectedClients.Count - 1
                : _networkManager.ConnectedClients.Count;

            while (_synchronousSpawnTimeoutTimer < NetworkSpawnTimeoutSeconds)
            {
                if (_synchronousSpawnAckCount >= requiredAcknowledgementsCount)
                {
                    Debug.Log(message: $"All clients have loaded the prefab in {_synchronousSpawnTimeoutTimer}" +
                                       "seconds, spawning the prefab on the server...");

                    return LoadAndSendPrefab(assetGuid);
                }

                _synchronousSpawnTimeoutTimer += Time.deltaTime;
                await UniTask.Yield();
            }

            // Left to the reader: you'll need to be reactive to clients failing to load -- you should either have
            // the offending client try again or disconnect it after a predetermined amount of failed attempts.
            Debug.LogError(message: "Failed to spawn dynamic prefab - timeout");

            SendError();
            return null;

            // LOCAL METHODS: -----------------------------

            GameObject LoadAndSendPrefab(AddressableGUID localAssetGuid)
            {
                if (!DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(localAssetGuid,
                        out AsyncOperationHandle<GameObject> prefab))
                {
                    Debug.LogWarning(message: $"GUID {localAssetGuid} is not a GUID of a previously loaded prefab. " +
                                              "Failed to spawn a prefab.");
                    return null;
                }

                // _networkManager.SpawnManager.InstantiateAndSpawn(prefabNetworkObject, NetworkHorror.ServerID,
                //     destroyWithScene: true, position: Vector3.zero);

                // Debug.Log(message: "Spawned dynamic prefab");

                // Every client loaded dynamic prefab, their respective ClientUIs in case they loaded first.
                foreach (ulong client in _networkManager.ConnectedClients.Keys)
                {
                    // m_InGameUI.ClientLoadedPrefabStatusChanged(client,
                    //     assetGuid.GetHashCode(),
                    //     prefab.Result.name,
                    //     InGameUI.LoadStatus.Loaded);
                }

                return prefab.Result;
            }

            void SendError() => Log.PrintError(log: "Error");
        }

        private async void TrySpawnInvisible()
        {
            int referencesAmount = _dynamicPrefabsReferences.Count;
            AssetReferenceGameObject randomPrefab = _dynamicPrefabsReferences[Random.Range(0, referencesAmount)];

            await SpawnImmediatelyAndHideUntilPrefabIsLoadedOnClient(
                guid: randomPrefab.AssetGUID,
                position: Random.insideUnitCircle * 5,
                rotation: Quaternion.identity
            );
        }

        /// <summary>
        /// This call spawns an addressable prefab by it's guid. It does not ensure that all the clients have loaded the
        /// prefab before spawning it. All spawned objects are network-invisible to clients that don't have the prefab
        /// loaded. The server tells the clients that lack the preloaded prefab to load it and acknowledge that they've
        /// loaded it, and then the server makes the object network-visible to that client.
        /// </summary>
        /// <returns></returns>
        private async UniTask<NetworkObject> SpawnImmediatelyAndHideUntilPrefabIsLoadedOnClient(string guid,
            Vector3 position, Quaternion rotation)
        {
            if (IsServer)
            {
                var assetGuid = new AddressableGUID()
                {
                    Value = guid
                };

                return await Spawn(assetGuid);
            }

            return null;

            async UniTask<NetworkObject> Spawn(AddressableGUID assetGuid)
            {
                // Server is starting to load a prefab, update UI.
                // m_InGameUI.ClientLoadedPrefabStatusChanged(NetworkManager.ServerClientId, assetGuid.GetHashCode(),
                //     "Undefined", InGameUI.LoadStatus.Loading);

                GameObject prefab = await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid,
                    ArtificialDelayMilliseconds);

                // Server loaded a prefab, update UI with the loaded asset's name.
                DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid,
                    out AsyncOperationHandle<GameObject> loadedGameObject);

                // m_InGameUI.ClientLoadedPrefabStatusChanged(NetworkManager.ServerClientId, assetGuid.GetHashCode(),
                //     loadedGameObject.Result.name, InGameUI.LoadStatus.Loaded);

                var obj = Instantiate(prefab, position, rotation).GetComponent<NetworkObject>();

                if (_prefabHashToNetworkObjectID.TryGetValue(assetGuid.GetHashCode(),
                        out HashSet<NetworkObject> networkObjectIds))
                {
                    networkObjectIds.Add(obj);
                }
                else
                {
                    _prefabHashToNetworkObjectID.Add(assetGuid.GetHashCode(), new HashSet<NetworkObject> { obj });
                }

                obj.CheckObjectVisibility = clientId =>
                {
                    // Object is loaded on the server, no need to validate for visibility.
                    if (clientId == NetworkManager.ServerClientId)
                        return true;

                    // If the client has already loaded the prefab - we can make the object network-visible to them.
                    if (DynamicPrefabLoadingUtilities.HasClientLoadedPrefab(clientId, assetGuid.GetHashCode()))
                        return true;

                    // Client is loading a prefab, update UI.
                    // m_InGameUI.ClientLoadedPrefabStatusChanged(clientId, assetGuid.GetHashCode(), "Undefined",
                    //     InGameUI.LoadStatus.Loading);

                    // Otherwise the client need to load the prefab, and after they ack - the ShowHiddenObjectsToClient.
                    LoadAddressableClientRpc(
                        assetGuid,
                        new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } }
                    );

                    return false;
                };

                obj.Spawn();

                return obj;
            }
        }

        private void ShowHiddenObjectsToClient(int prefabHash, ulong clientId)
        {
            if (!_prefabHashToNetworkObjectID.TryGetValue(prefabHash, out HashSet<NetworkObject> networkObjects))
                return;

            foreach (NetworkObject obj in networkObjects)
            {
                if (obj.IsNetworkVisibleTo(clientId))
                    continue;

                obj.NetworkShow(clientId);
            }
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void AcknowledgeSuccessfulPrefabLoadServerRpc(int prefabHash, ServerRpcParams rpcParams = default)
        {
            _synchronousSpawnAckCount++;

            Debug.Log(message: $"Client acknowledged successful prefab load with hash: {prefabHash}");

            DynamicPrefabLoadingUtilities.RecordThatClientHasLoadedAPrefab(prefabHash,
                rpcParams.Receive.SenderClientId);

            // The server has all the objects network-visible, no need to do anything.
            if (rpcParams.Receive.SenderClientId != _networkManager.LocalClientId)
            {
                // Note: there's a potential security risk here if this technique is tied with gameplay that uses
                // a NetworkObject's Show() and Hide() methods. For example, a malicious player could invoke a similar
                // ServerRpc with the guids of enemy players, and it would make those enemies visible (network side
                // and/or visually) to that player, giving them a potential advantage.
                ShowHiddenObjectsToClient(prefabHash, rpcParams.Receive.SenderClientId);
            }

            // A quick way to grab a matching prefab reference's name via its prefabHash.
            string loadedPrefabName = "Undefined";

            foreach (AssetReferenceGameObject prefabReference in _dynamicPrefabsReferences)
            {
                var prefabReferenceGuid = new AddressableGUID { Value = prefabReference.AssetGUID };

                if (prefabReferenceGuid.GetHashCode() != prefabHash)
                    continue;

                // Found the matching prefab reference.
                if (DynamicPrefabLoadingUtilities.TryGetLoadedDynamicPrefabResourceHandle(prefabReferenceGuid,
                        out AsyncOperationHandle<GameObject> loadedGameObject))
                {
                    // If it is loaded on the server, update the name on the ClientUI.
                    loadedPrefabName = loadedGameObject.Result.name;
                }

                break;
            }

            // Client has successfully loaded a prefab, update UI.
            // m_InGameUI.ClientLoadedPrefabStatusChanged(rpcParams.Receive.SenderClientId, prefabHash, loadedPrefabName,
            //     InGameUI.LoadStatus.Loaded);
        }

        [ClientRpc]
        private void LoadAddressableClientRpc(AddressableGUID guid, ClientRpcParams rpcParams = default)
        {
            if (!IsHost)
                Load(guid);

            async void Load(AddressableGUID assetGuid)
            {
                // Loading prefab as a client, update UI.
                // m_InGameUI.ClientLoadedPrefabStatusChanged(m_NetworkManager.LocalClientId, assetGuid.GetHashCode(),
                //     "Undefined", InGameUI.LoadStatus.Loading);

                Debug.Log(message: "Loading dynamic prefab on the client...");

                await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid, ArtificialDelayMilliseconds);

                Debug.Log(message: "Client loaded dynamic prefab");

                DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid,
                    out AsyncOperationHandle<GameObject> loadedGameObject);

                // m_InGameUI.ClientLoadedPrefabStatusChanged(m_NetworkManager.LocalClientId, assetGuid.GetHashCode(),
                //     loadedGameObject.Result.name, InGameUI.LoadStatus.Loaded);

                AcknowledgeSuccessfulPrefabLoadServerRpc(assetGuid.GetHashCode());
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTrySpawnGameObjectPrefab(string guid, Action<GameObject> callback) =>
            TryLoadAndGetDynamicGameObjectPrefab(guid, callback);

        private void OnTrySpawnNetworkObjectPrefab(string guid, Action<NetworkObject> callback) =>
            TryLoadAndGetDynamicNetworkObjectPrefab(guid, callback);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Button]
        private void OnClickedPreload()
        {
            if (!_networkManager.IsServer)
                return;

            PreloadPrefabs().Forget();
        }

        [Button]
        private void OnClickedTrySpawnInvisible()
        {
            if (!_networkManager.IsServer)
                return;

            TrySpawnInvisible();
        }
    }
}