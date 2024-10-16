﻿using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Network.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace GameCore.Gameplay.Network.DynamicPrefabs
{
    /// <summary>
    /// A utilities class to handle the loading, tracking, and disposing of loaded network prefabs. Connection and
    /// disconnection payloads can be easily accessed from this class as well.
    /// </summary>
    /// <remarks>
    /// Artificial delay to the loading of a network prefab is disabled by default. To enable it, make sure to add
    /// ENABLE_ARTIFICIAL_DELAY as a scripting define symbol to your project's Player settings.
    /// </remarks>
    public static class DynamicPrefabLoadingUtilities
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        static DynamicPrefabLoadingUtilities()
        {
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public static int HashOfDynamicPrefabGUIDs { get; private set; } = EmptyDynamicPrefabHash;
        public static int LoadedPrefabCount => CompletedCache.Count;
        //public static int LoadedPrefabCount => LoadedDynamicPrefabResourceHandles.Count;

        // FIELDS: --------------------------------------------------------------------------------

        private const int EmptyDynamicPrefabHash = -1;

        //private static readonly Dictionary<AddressableGUID, AsyncOperationHandle<GameObject>>
            //LoadedDynamicPrefabResourceHandles = new(new AddressableGUIDEqualityComparer());

        private static readonly Dictionary<AddressableGUID, AsyncOperationHandle<GameObject>> CompletedCache =
            new(new AddressableGUIDEqualityComparer());

        private static readonly Dictionary<AddressableGUID, List<AsyncOperationHandle<GameObject>>> Handles =
            new(new AddressableGUIDEqualityComparer());

        // A storage where we keep the association between the dynamic prefab (hash of it's GUID)
        // and the clients that have it loaded.
        private static readonly Dictionary<int, HashSet<ulong>> PrefabHashToClientIds = new();

        // Cached list to avoid GC.
        private static readonly List<AddressableGUID> DynamicPrefabGUIDs = new();

        private static NetworkManager _networkManager;
        private static DiContainer _diContainer;
        
        public static async UniTask<GameObject> LoadAsset(AddressableGUID addressableGUID)
        {
            if (CompletedCache.TryGetValue(addressableGUID, out var completedHandle))
                return completedHandle.Result;

            var handle = Addressables.LoadAssetAsync<GameObject>(key: addressableGUID.ToString());
            return await RunWitchCacheOnComplete(handle, addressableGUID);
        }
        
        private static async UniTask<GameObject> RunWitchCacheOnComplete(AsyncOperationHandle<GameObject> handle,
            AddressableGUID addressableGUID)
        {
            handle.Completed += operationHandle => { CompletedCache[addressableGUID] = operationHandle; };

            AddHandle(addressableGUID, handle);

            return await handle.Task;
        }
        
        private static void AddHandle(AddressableGUID addressableGUID, AsyncOperationHandle<GameObject> handle)
        {
            if (!Handles.TryGetValue(addressableGUID, out List<AsyncOperationHandle<GameObject>> resourceHandles))
            {
                resourceHandles = new List<AsyncOperationHandle<GameObject>>();
                Handles[addressableGUID] = resourceHandles;
            }

            resourceHandles.Add(handle);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void Init(NetworkManager networkManager) =>
            _networkManager = networkManager;

        public static void SetDiContainer(DiContainer diContainer) =>
            _diContainer = diContainer;

        public static async UniTask<IList<GameObject>> LoadDynamicPrefabs(AddressableGUID[] guids,
            int artificialDelaySeconds = 0)
        {
            var tasks = new List<UniTask<GameObject>>();

            foreach (AddressableGUID guid in guids)
            {
                UniTask<GameObject> prefab = LoadDynamicPrefab(guid, artificialDelaySeconds, recomputeHash: false);
                tasks.Add(prefab);
            }

            GameObject[] prefabs = await UniTask.WhenAll(tasks);
            CalculateDynamicPrefabArrayHash();

            return prefabs;
        }

        public static async UniTask<GameObject> LoadDynamicPrefab(AddressableGUID guid,
            int artificialDelayMilliseconds = 0, bool recomputeHash = true, bool registerAtNetwork = true)
        {
            // if (LoadedDynamicPrefabResourceHandles.ContainsKey(guid))
            // {
            //     Debug.Log(message: $"Prefab has already been loaded, skipping loading this time | {guid}");
            //     return LoadedDynamicPrefabResourceHandles[guid].Result;
            // }
            //
            // Debug.Log(message: $"Loading dynamic prefab {guid.Value}");
            //
            // var asyncOperation = Addressables.LoadAssetAsync<GameObject>(key: guid.ToString());
            // LoadedDynamicPrefabResourceHandles.Add(guid, asyncOperation);

            bool containsInCache = CompletedCache.ContainsKey(guid);
            bool containsInHandles = Handles.ContainsKey(guid);
            bool register = !containsInCache && !containsInHandles;
            
            GameObject prefab =  await LoadAsset(guid);

#if ENABLE_ARTIFICIAL_DELAY
            // THIS IS FOR EDUCATIONAL PURPOSES AND SHOULDN'T BE INCLUDED IN YOUR PROJECT
            await UniTask.Delay(artificialDelayMilliseconds);
#endif

            if (registerAtNetwork && register)
            {
                _networkManager.AddNetworkPrefab(prefab);

                _networkManager.PrefabHandler.AddHandler(
                    networkPrefabAsset: prefab,
                    instanceHandler: new ZenjectNetCodeFactory(prefab, _diContainer)
                );
            }

            if (recomputeHash)
                CalculateDynamicPrefabArrayHash();

            return prefab;
        }

        public static bool TryGetLoadedGameObjectFromGuid(AddressableGUID assetGuid,
            out AsyncOperationHandle<GameObject> loadedGameObject)
        {
            return CompletedCache.TryGetValue(assetGuid, out loadedGameObject);
            // return LoadedDynamicPrefabResourceHandles.TryGetValue(assetGuid, out loadedGameObject);
        }

        /// <remarks>
        /// This is not the most optimal algorithm for big quantities of Addressables, but easy enough to maintain for a
        /// small number like in this sample. One could use a "client dirty" algorithm to mark clients needing loading
        /// or not instead, but that would require more complex dirty management.
        /// </remarks>
        public static void RecordThatClientHasLoadedAllPrefabs(ulong clientId)
        {
            foreach (AddressableGUID dynamicPrefabGUID in DynamicPrefabGUIDs)
            {
                int hashCode = dynamicPrefabGUID.GetHashCode();
                RecordThatClientHasLoadedAPrefab(hashCode, clientId);
            }
        }

        public static void RecordThatClientHasLoadedAPrefab(int assetGuidHash, ulong clientId)
        {
            if (PrefabHashToClientIds.TryGetValue(assetGuidHash, out HashSet<ulong> clientIds))
                clientIds.Add(clientId);
            else
                PrefabHashToClientIds.Add(assetGuidHash, new HashSet<ulong> { clientId });
        }

        public static void RefreshLoadedPrefabGuids()
        {
            DynamicPrefabGUIDs.Clear();
            DynamicPrefabGUIDs.AddRange(CompletedCache.Keys);
            //DynamicPrefabGUIDs.AddRange(LoadedDynamicPrefabResourceHandles.Keys);
        }

        public static void UnloadAndReleaseAllDynamicPrefabs()
        {
            if (_networkManager == null)
                return;
            
            HashOfDynamicPrefabGUIDs = EmptyDynamicPrefabHash;

            foreach (var pair in Handles)
            {
                foreach (var handle in pair.Value)
                {
                    _networkManager.RemoveNetworkPrefab(handle.Result);
                    Addressables.Release(handle);
                }
            }

            CompletedCache.Clear();
            Handles.Clear();
        }

        /// <remarks>
        /// Testing showed that with the current implementation, Netcode for GameObjects will send the DisconnectReason
        /// message as a non-fragmented message, meaning that the upper limit of this message in bytes is exactly
        /// NON_FRAGMENTED_MESSAGE_MAX_SIZE bytes (1300 at the time of writing), defined inside of
        /// <see cref="MessagingSystem"/>.
        /// For this reason, DisconnectReason should only be used to instruct the user "why" a connection failed, and
        /// "where" to fetch the relevant connection data. We recommend using services like UGS to fetch larger batches
        /// of data.
        /// </remarks>
        public static string GenerateDisconnectionPayload()
        {
            var dynamicPrefabGuidStrings = new List<string>();

            foreach (AddressableGUID dynamicPrefabGuid in DynamicPrefabGUIDs)
                dynamicPrefabGuidStrings.Add(item: dynamicPrefabGuid.ToString());

            var rejectionPayload = new DisconnectionPayload
            {
                reason = DisconnectReason.ClientNeedsToPreload,
                guids = dynamicPrefabGuidStrings
            };

            return JsonUtility.ToJson(rejectionPayload);
        }

        public static byte[] GenerateRequestPayload()
        {
            string payload = JsonUtility.ToJson(obj: new ConnectionPayload()
            {
                hashOfDynamicPrefabGUIDs = HashOfDynamicPrefabGUIDs
            });

            return Encoding.UTF8.GetBytes(payload);
        }

        public static bool TryGetLoadedDynamicPrefabResourceHandle(AddressableGUID addressableGUID,
            out AsyncOperationHandle<GameObject> result)
        {
            return CompletedCache.TryGetValue(addressableGUID, out result);
            //return LoadedDynamicPrefabResourceHandles.TryGetValue(addressableGUID, out result);
        }

        public static bool HasClientLoadedPrefab(ulong clientId, int prefabHash) =>
            PrefabHashToClientIds.TryGetValue(prefabHash, out HashSet<ulong> clientIds) && clientIds.Contains(clientId);

        public static bool IsPrefabLoadedOnAllClients(AddressableGUID assetGuid) =>
            CompletedCache.ContainsKey(assetGuid);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void CalculateDynamicPrefabArrayHash()
        {
            // We need to sort the array so that the hash is consistent across clients.
            // It's possible to use an order-independent hashing algorithm for some potential performance gains.
            RefreshLoadedPrefabGuids();
            DynamicPrefabGUIDs.Sort(comparison: (addressableGUID, guid) => addressableGUID.Value.CompareTo(guid.Value));
            HashOfDynamicPrefabGUIDs = EmptyDynamicPrefabHash;

            // A simple hash combination algorithm suggested by Jon Skeet,
            // found here: https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            // We can't use C# HashCode combine because it is unreliable across different processes (by design).
            unchecked
            {
                int hash = 17;

                for (var i = 0; i < DynamicPrefabGUIDs.Count; ++i)
                    hash = hash * 31 + DynamicPrefabGUIDs[i].GetHashCode();

                HashOfDynamicPrefabGUIDs = hash;
            }

            // Debug.Log(message: $"Calculated hash of dynamic prefabs: {HashOfDynamicPrefabGUIDs}");
        }
    }
}