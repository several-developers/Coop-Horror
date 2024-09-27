﻿using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GameCore.Gameplay.Network
{
    public class AddressablesSceneManagerHandler : ISceneManagerHandler
    {
        private Scene m_InvalidScene = new();

        internal struct SceneEntry
        {
            public bool IsAssigned;
            public Scene Scene;
        }
        
        internal Dictionary<string, Dictionary<int, SceneEntry>> SceneNameToSceneHandles = new Dictionary<string, Dictionary<int, SceneEntry>>();

        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> loadOperationHandles = new();

        public AddressablesSceneManagerHandler() => SceneManager.sceneLoaded += RemoveOperationHandle;

        ~AddressablesSceneManagerHandler() => SceneManager.sceneLoaded -= RemoveOperationHandle;

        private void RemoveOperationHandle(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (loadSceneMode == LoadSceneMode.Single)
            {
                loadOperationHandles.Remove(scene.name);
            }
        }

        public AsyncOperationHandle<SceneInstance> PreloadSceneAsync(string sceneName, LoadSceneMode loadSceneMode) =>
            loadOperationHandles[sceneName] =
                Addressables.LoadSceneAsync(sceneName, loadSceneMode, activateOnLoad: false);

        public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode,
            SceneEventProgress sceneEventProgress)
        {
            var activated = false;
            var hook = sceneEventProgress.GetAsyncOperationCompletionHook(check: () => activated);

            LoadSceneAsync(sceneName, loadSceneMode, onActivated: () =>
            {
                activated = true;
                hook?.Invoke();
            });

            return null;
        }

        public AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode, Action onActivated = null)
        {
            if (!loadOperationHandles.TryGetValue(sceneName, out var loadOperationHandle))
            {
                loadOperationHandle = PreloadSceneAsync(sceneName, loadSceneMode);
            }

            loadOperationHandle.Completed += operationHandle =>
                operationHandle.Result.ActivateAsync().completed += _ => onActivated?.Invoke();

            return null;
        }

        public AsyncOperation UnloadSceneAsync(Scene scene, SceneEventProgress sceneEventProgress)
        {
            if (loadOperationHandles.TryGetValue(scene.name, out var loadOperationHandle))
            {
                var unloadOperationHandle = Addressables.UnloadSceneAsync(loadOperationHandle);

                unloadOperationHandle.Completed += _ =>
                    sceneEventProgress.GetAsyncOperationCompletionHook(check: () => unloadOperationHandle.IsDone)
                        .Invoke();

                loadOperationHandles.Remove(scene.name);
            }

            return null;
        }

        /// <summary>
        /// Only invoked is client synchronization is additive, this will generate the scene tracking table
        /// in order to re-use the same scenes the server is synchronizing instead of having to unload the
        /// scenes and reload them when synchronizing (i.e. client disconnects due to external reason, the
        /// same application instance is still running, the same scenes are still loaded on the client, and
        /// upon reconnecting the client doesn't have to unload the scenes and then reload them)
        /// </summary>
        public void PopulateLoadedScenes(ref Dictionary<int, Scene> scenesLoaded, NetworkManager networkManager)
        {
            SceneNameToSceneHandles.Clear();
            var sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!SceneNameToSceneHandles.ContainsKey(scene.name))
                {
                    SceneNameToSceneHandles.Add(scene.name, new Dictionary<int, SceneEntry>());
                }

                if (!SceneNameToSceneHandles[scene.name].ContainsKey(scene.handle))
                {
                    var sceneEntry = new SceneEntry()
                    {
                        IsAssigned = false,
                        Scene = scene
                    };
                    SceneNameToSceneHandles[scene.name].Add(scene.handle, sceneEntry);
                    if (!scenesLoaded.ContainsKey(scene.handle))
                    {
                        scenesLoaded.Add(scene.handle, scene);
                    }
                }
                else
                {
                    throw new Exception($"[Duplicate Handle] Scene {scene.name} already has scene handle {scene.handle} registered!");
                }
            }
        }

        /// <summary>
        /// This will find any scene entry that hasn't been used/assigned, set the entry to assigned, and
        /// return the associated scene. If none are found it returns an invalid scene.
        /// </summary>
        public Scene GetSceneFromLoadedScenes(string sceneName, NetworkManager networkManager)
        {
            if (SceneNameToSceneHandles.ContainsKey(sceneName))
            {
                foreach (var sceneHandleEntry in SceneNameToSceneHandles[sceneName])
                {
                    if (!sceneHandleEntry.Value.IsAssigned)
                    {
                        var sceneEntry = sceneHandleEntry.Value;
                        sceneEntry.IsAssigned = true;
                        SceneNameToSceneHandles[sceneName][sceneHandleEntry.Key] = sceneEntry;
                        return sceneEntry.Scene;
                    }
                }
            }
            // If we found nothing return an invalid scene
            return m_InvalidScene;
        }

        /// <summary>
        /// Determines if there is an existing scene loaded that matches the scene name but has not been assigned
        /// </summary>
        public bool DoesSceneHaveUnassignedEntry(string sceneName, NetworkManager networkManager)
        {
            var scenesWithSceneName = new List<Scene>();

            // Get all loaded scenes with the same name
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName)
                {
                    scenesWithSceneName.Add(scene);
                }
            }

            // If there are no scenes of this name loaded then we have no loaded scenes
            // to use
            if (scenesWithSceneName.Count == 0)
            {
                return false;
            }

            // If we have 1 or more scenes with the name and we have no entries, then we do have
            // a scene to use
            if (scenesWithSceneName.Count > 0 && !SceneNameToSceneHandles.ContainsKey(sceneName))
            {
                return true;
            }

            // Determine if any of the loaded scenes has been used for synchronizing
            foreach (var scene in scenesWithSceneName)
            {
                // If we don't have the handle, then we can use that scene
                if (!SceneNameToSceneHandles[scene.name].ContainsKey(scene.handle))
                {
                    return true;
                }

                // If we have an entry, but it is not yet assigned (i.e. preloaded)
                // then we can use that.
                if (!SceneNameToSceneHandles[scene.name][scene.handle].IsAssigned)
                {
                    return true;
                }
            }
            // If none were found, then we have no available scene (which most likely means one will get loaded)
            return false;
        }

        /// <summary>
        /// Stops tracking a specific scene
        /// </summary>
        public void StopTrackingScene(int handle, string name, NetworkManager networkManager)
        {
            if (SceneNameToSceneHandles.ContainsKey(name))
            {
                if (SceneNameToSceneHandles[name].ContainsKey(handle))
                {
                    SceneNameToSceneHandles[name].Remove(handle);
                    if (SceneNameToSceneHandles[name].Count == 0)
                    {
                        SceneNameToSceneHandles.Remove(name);
                    }
                }
            }
        }

        /// <summary>
        /// Starts tracking a specific scene
        /// </summary>
        public void StartTrackingScene(Scene scene, bool assigned, NetworkManager networkManager)
        {
            if (!SceneNameToSceneHandles.ContainsKey(scene.name))
            {
                SceneNameToSceneHandles.Add(scene.name, new Dictionary<int, SceneEntry>());
            }

            if (!SceneNameToSceneHandles[scene.name].ContainsKey(scene.handle))
            {
                var sceneEntry = new SceneEntry()
                {
                    IsAssigned = true,
                    Scene = scene
                };
                SceneNameToSceneHandles[scene.name].Add(scene.handle, sceneEntry);
            }
            else
            {
                throw new Exception($"[Duplicate Handle] Scene {scene.name} already has scene handle {scene.handle} registered!");
            }
        }
        
        /// <summary>
        /// Resets scene tracking
        /// </summary>
        public void ClearSceneTracking(NetworkManager networkManager)
        {
            SceneNameToSceneHandles.Clear();
        }
        
        private List<Scene> m_ScenesToUnload = new List<Scene>();

        /// <summary>
        /// Unloads any scenes that have not been assigned.
        /// </summary>
        /// <param name="networkManager"></param>
        public void UnloadUnassignedScenes(NetworkManager networkManager = null)
        {
            var sceneManager = networkManager.SceneManager;
            SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
            foreach (var sceneEntry in SceneNameToSceneHandles)
            {
                var scenHandleEntries = SceneNameToSceneHandles[sceneEntry.Key];
                foreach (var sceneHandleEntry in scenHandleEntries)
                {
                    if (!sceneHandleEntry.Value.IsAssigned)
                    {
                        if (sceneManager.VerifySceneBeforeUnloading == null || sceneManager.VerifySceneBeforeUnloading.Invoke(sceneHandleEntry.Value.Scene))
                        {
                            m_ScenesToUnload.Add(sceneHandleEntry.Value.Scene);
                        }
                    }
                }
            }
            foreach (var sceneToUnload in m_ScenesToUnload)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
                // Update the ScenesLoaded when we unload scenes
                if (sceneManager.ScenesLoaded.ContainsKey(sceneToUnload.handle))
                {
                    sceneManager.ScenesLoaded.Remove(sceneToUnload.handle);
                }
            }
        }

        /// <summary>
        /// Handles migrating dynamically spawned NetworkObjects to the DDOL when a scene is unloaded
        /// </summary>
        /// <param name="networkManager"><see cref="NetworkManager"/>relative instance</param>
        /// <param name="scene">scene being unloaded</param>
        public void MoveObjectsFromSceneToDontDestroyOnLoad(ref NetworkManager networkManager, Scene scene)
        {
            bool isActiveScene = scene == SceneManager.GetActiveScene();
            // Create a local copy of the spawned objects list since the spawn manager will adjust the list as objects
            // are despawned.
            var localSpawnedObjectsHashSet = new HashSet<NetworkObject>(networkManager.SpawnManager.SpawnedObjectsList);
            foreach (var networkObject in localSpawnedObjectsHashSet)
            {
                if (networkObject == null || (networkObject != null && networkObject.gameObject.scene.handle != scene.handle))
                {
                    continue;
                }

                // Only NetworkObjects marked to not be destroyed with the scene and are not already in the DDOL are preserved
                if (!networkObject.DestroyWithScene && networkObject.gameObject.scene != networkManager.SceneManager.DontDestroyOnLoadScene)
                {
                    // Only move dynamically spawned NetworkObjects with no parent as the children will follow
                    if (networkObject.gameObject.transform.parent == null && networkObject.IsSceneObject != null && !networkObject.IsSceneObject.Value)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(networkObject.gameObject);
                    }
                }
                else if (networkManager.IsServer)
                {
                    networkObject.Despawn();
                }
                else // We are a client, migrate the object into the DDOL temporarily until it receives the destroy command from the server
                {
                    UnityEngine.Object.DontDestroyOnLoad(networkObject.gameObject);
                }
            }
        }

        /// <summary>
        /// Sets the client synchronization mode which impacts whether both the server or client take into consideration scenes loaded before
        /// starting the <see cref="NetworkManager"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="LoadSceneMode.Single"/>: Does not take preloaded scenes into consideration
        /// <see cref="LoadSceneMode.Single"/>: Does take preloaded scenes into consideration
        /// </remarks>
        /// <param name="networkManager">relative <see cref="NetworkManager"/> instance</param>
        /// <param name="mode"><see cref="LoadSceneMode.Single"/> or <see cref="LoadSceneMode.Additive"/></param>
        public void SetClientSynchronizationMode(ref NetworkManager networkManager, LoadSceneMode mode)
        {
            var sceneManager = networkManager.SceneManager;
            // Don't let client's set this value
            if (!networkManager.IsServer)
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    NetworkLog.LogWarning("Clients should not set this value as it is automatically synchronized with the server's setting!");
                }
                return;
            }
            else // Warn users if they are changing this after there are clients already connected and synchronized
            if (networkManager.ConnectedClientsIds.Count > (networkManager.IsHost ? 1 : 0) && sceneManager.ClientSynchronizationMode != mode)
            {
                if (NetworkLog.CurrentLogLevel <= LogLevel.Normal)
                {
                    NetworkLog.LogWarning("Server is changing client synchronization mode after clients have been synchronized! It is recommended to do this before clients are connected!");
                }
            }

            // For additive client synchronization, we take into consideration scenes
            // already loaded.
            if (mode == LoadSceneMode.Additive)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);

                    // If using scene verification
                    if (sceneManager.VerifySceneBeforeLoading != null)
                    {
                        // Determine if we should take this scene into consideration
                        if (!sceneManager.VerifySceneBeforeLoading.Invoke(scene.buildIndex, scene.name, LoadSceneMode.Additive))
                        {
                            continue;
                        }
                    }

                    // If the scene is not already in the ScenesLoaded list, then add it
                    if (!sceneManager.ScenesLoaded.ContainsKey(scene.handle))
                    {
                        sceneManager.ScenesLoaded.Add(scene.handle, scene);
                    }
                }
            }
            // Set the client synchronization mode
            sceneManager.ClientSynchronizationMode = mode;
        }

        /// <summary>
        /// Handles determining if a client should attempt to load a scene during synchronization.
        /// </summary>
        /// <param name="sceneName">name of the scene to be loaded</param>
        /// <param name="isPrimaryScene">when in client synchronization mode single, this determines if the scene is the primary active scene</param>
        /// <param name="clientSynchronizationMode">the current client synchronization mode</param>
        /// <param name="networkManager"><see cref="NetworkManager"/> instance</param>
        /// <returns></returns>
        public bool ClientShouldPassThrough(string sceneName, bool isPrimaryScene, LoadSceneMode clientSynchronizationMode, NetworkManager networkManager)
        {
            var shouldPassThrough = clientSynchronizationMode == LoadSceneMode.Single ? false : DoesSceneHaveUnassignedEntry(sceneName, networkManager);
            var activeScene = SceneManager.GetActiveScene();

            // If shouldPassThrough is not yet true and the scene to be loaded is the currently active scene
            if (!shouldPassThrough && sceneName == activeScene.name)
            {
                // In additive mode we always pass through, but in LoadSceneMode.Single we only pass through if the currently active scene
                // is the primary scene to be loaded
                if (clientSynchronizationMode == LoadSceneMode.Additive || (isPrimaryScene && clientSynchronizationMode == LoadSceneMode.Single))
                {
                    // don't try to reload this scene and pass through to post load processing.
                    shouldPassThrough = true;
                }
            }
            return shouldPassThrough;
        }
        
        private void SceneManager_SceneUnloaded(Scene scene)
        {
            if (SceneNameToSceneHandles.ContainsKey(scene.name))
            {
                if (SceneNameToSceneHandles[scene.name].ContainsKey(scene.handle))
                {
                    SceneNameToSceneHandles[scene.name].Remove(scene.handle);
                }
                if (SceneNameToSceneHandles[scene.name].Count == 0)
                {
                    SceneNameToSceneHandles.Remove(scene.name);
                }
                m_ScenesToUnload.Remove(scene);
                if (m_ScenesToUnload.Count == 0)
                {
                    SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
                }
            }
        }
    }
}