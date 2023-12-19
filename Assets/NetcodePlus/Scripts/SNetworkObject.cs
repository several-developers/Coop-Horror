using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace NetcodePlus
{
    /// <summary>
    /// Improved version of NetworkObject
    /// Use only with SNetworkBehaviour, do not add both NetworkObject and SNetworkObject on the same object
    /// </summary>

    public class SNetworkObject : MonoBehaviour
    {
        public AutoSpawnType _autoSpawn;        //Object will be spawned automatically when client is Ready
        public ulong _networkID;                //ID to access the instantiated object, this id should match on all clients/server
        public ulong _prefabID;                 //ID to access the prefab of this object, this id should match on all clients/server
        public bool _isScene;                   //Object was placed in the scene and not instantiated, this means it already exists on both client and server

        public UnityAction onReady;             //Called after connection was fully established (and all data loaded)
        public UnityAction onBeforeSpawn;       //Called before the object is spawned (on the server only)
        public UnityAction onSpawn;             //Called when this object is spawned (means that the syncing with clients starts)
        public UnityAction onDespawn;           //Called when this object is despawned (means that the syncing with clients stop)

        // FIELDS: --------------------------------------------------------------------------------
        
        private ulong _owner;                   //Owner of this object, 0 is usually the server
        private bool _isReady;                  //If the object is ready (connection was extablished and OnReady was called)
        private bool _isSpawned;                //If the object is spawned or not (spawn means it has been sent to clients for syncing)
        private bool _isDestroying;             //Object is being destroyed, prevent spawning

        private SNetworkOptimizer _optimizer;   //Can be null, will automatically spawn/despawn this object based on distance to players
        private SNetworkBehaviour[] _behaviours;//List of behaviours
        private bool _registered;               //Prevent registering twice

        private static readonly List<SNetworkObject> NetObjects = new();

        public static bool EditorAutoGenID = true; //Set to off to turn off auto id generation

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            NetObjects.Add(item: this);
            _optimizer = GetComponent<SNetworkOptimizer>();
            _behaviours = GetComponentsInChildren<SNetworkBehaviour>();

            //Set behaviours id
            for (ushort i = 0; i < _behaviours.Length; i++)
                _behaviours[i].SetBehaviourId(i);

            if (!_isScene)
                GenerateID(); //Generate ID for newly created objects
            if(_networkID == 0)
                Debug.Log("Network ID is 0 " + gameObject.name);
        }

        protected virtual void Start()
        {
            RegisterScene();
            TriggerReady();
        }

        public virtual void OnDestroy()
        {
            _isDestroying = true;
            NetObjects.Remove(this);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void GenerateID() =>
            _networkID = NetworkTool.GenerateRandomUInt64();

        public void GenerateEditorID()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return; //Ignore in play mode

            GlobalObjectId gobjid = GlobalObjectId.GetGlobalObjectIdSlow(this);
            if (gobjid.targetObjectId == 0 || gobjid.assetGUID.Empty())
                return; //Invalid id, ignore for now

            ulong prevNid = _networkID;
            ulong prevPid = _prefabID;
            bool pscene = _isScene;

            string stringID = gobjid.ToString();
            ulong id = NetworkTool.Hash64(stringID);
            _networkID = id;
            _prefabID = id;
            _isScene = (gameObject.scene.rootCount > 0);

            if (_isScene)
            {
                _prefabID = 0;
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
                SNetworkObject nprefab = prefab != null ? prefab.GetComponent<SNetworkObject>() : null;
                
                if (nprefab != null)
                    _prefabID = nprefab._prefabID;
            }

            if (_prefabID != prevPid || _networkID != prevNid || _isScene != pscene)
                EditorUtility.SetDirty(this);
#endif
        }

        public virtual void Spawn() =>
            Spawn(TheNetwork.Get().ServerID);

        public virtual void Spawn(ulong owner)
        {
            if (_isDestroying)
                return; //Object already being destroyed

            if (_networkID == 0 && IsServer)
                GenerateID();

            if (!IsServer || IsSpawned)
                return;
            
            //Debug.Log("Spawn " +gameObject.name);
            _owner = owner;
            _isSpawned = true;
            onBeforeSpawn?.Invoke();
            NetworkSpawner.Get().Spawn(this);
            onSpawn?.Invoke();
        }

        public virtual void Despawn(bool destroy = false)
        {
            if (_isDestroying)
                return; //Object being destroyed

            if (!IsServer || !IsSpawned)
                return;
            
            //Debug.Log("Despawn " + gameObject.name);
            _isSpawned = false;
            _isDestroying = destroy;
            NetworkSpawner.Get().Despawn(this, destroy);
            onDespawn?.Invoke();

            if (destroy)
                Destroy(gameObject);
        }

        public virtual void DestroyUnspawned()
        {
            if (IsSpawned || _isDestroying)
                return;
            
            _isDestroying = true;
            NetworkSpawner.Get().DestroyScene(this);
            Destroy(gameObject);
        }

        public virtual void Destroy()
        {
            if (_isDestroying)
                return; //Object already being destroyed

            if (IsServer && IsSpawned)
                Despawn(true);
            else if (IsSpawned)
                DespawnLocal(true);
            else
                DestroyUnspawned();
        }

        public virtual void Destroy(float delay)
        {
            if (delay > 0.01f)
                TimeTool.WaitFor(delay, Destroy);
            else
                Destroy();
        }

        public void AutoSpawnOrDestroy()
        {
            if (IsServer)
            {
                if (!IsSceneObject && _autoSpawn == AutoSpawnType.SceneOnly)
                    return;

                if (_autoSpawn == AutoSpawnType.Never)
                    return;

                if (IsActive())
                    Spawn();
            }
            else
            {
                if (IsSceneObject && !IsSpawned)
                    SetActive(false); //Wait for object to be spawned before showing
            }
        }

        public void SpawnLocal(ulong owner, byte[] extra)
        {
			if (_isDestroying)
                return; //Object being destroyed
			
            if (!_isSpawned)
            {
                _isSpawned = true;
                this._owner = owner;
                ReadBehaviorSpawnData(extra);
                SetActive(true);
                onSpawn?.Invoke();
            }
        }

        public void DespawnLocal(bool destroy = false)
        {
			if (_isDestroying)
                return; //Object being destroyed
			
            if (_isSpawned)
            {
                _isSpawned = false;
                SetActive(false);
                onDespawn?.Invoke();
            }
			
			_isDestroying = destroy;
			if (destroy)
                Destroy(gameObject);
        }

        public void ChangeOwner(ulong owner)
        {
            if (IsSpawned && this._owner != owner)
            {
                this._owner = owner;
                NetworkSpawner.Get().ChangeOwner(this);
            }
        }
        
        public void RegisterScene()
        {
            if (_isScene && !_registered)
            {
                _registered = true;
                NetworkGame.Get().Spawner.RegisterSceneObject(this);
            }
        }

        public void TriggerReady()
        {
            if (!_isReady && TheNetwork.Get().IsReady())
            {
                _isReady = true;
                onReady?.Invoke();
                AutoSpawnOrDestroy();
            }
        }

        public byte[] WriteBehaviorSpawnData()
        {
            //Check if any behavior have spawn data
            List<SNetworkBehaviour> beha_list = new List<SNetworkBehaviour>();
            foreach (SNetworkBehaviour beha in _behaviours)
            {
                if (beha.GetSpawnData().Length > 0)
                    beha_list.Add(beha);
            }

            if (beha_list.Count == 0)
                return new byte[0];

            //Write the ones with data
            FastBufferWriter writer = new FastBufferWriter(128, Allocator.Temp, TheNetwork.MsgSize);
            writer.WriteValueSafe((ushort)beha_list.Count);
            foreach (SNetworkBehaviour beha in beha_list)
            {
                byte[] bextra = beha.GetSpawnData();
                writer.TryBeginWrite(4 + bextra.Length);
                writer.WriteValue(beha.BehaviourId);
                writer.WriteValue((ushort)bextra.Length);
                writer.WriteBytes(bextra);
            }
            byte[] wextra = writer.ToArray();
            writer.Dispose();
            return wextra;
        }

        public void ReadBehaviorSpawnData(byte[] bextra)
        {
            if (bextra.Length == 0)
                return;

            FastBufferReader reader = new FastBufferReader(bextra, Allocator.Temp);
            reader.ReadValueSafe(out ushort count);
            for(int i=0; i<count; i++)
            {
                reader.TryBeginRead(4);
                reader.ReadValue(out ushort beha_id);
                reader.ReadValue(out ushort byte_length);
                byte[] beha_extra = new byte[byte_length];
                reader.ReadBytesSafe(ref beha_extra, byte_length);

                if (beha_id >= 0 && beha_id < _behaviours.Length)
                    _behaviours[beha_id].SetSpawnData(beha_extra);
            }
            reader.Dispose();
        }

        public SNetworkBehaviour GetBehaviour(ushort id)
        {
            foreach (SNetworkBehaviour beha in _behaviours)
            {
                if (beha.BehaviourId == id)
                    return beha;
            }
            return null;
        }

        public float GetPlayerRange(float range_max = 99999f)
        {
            //Useful for optimization purpose, check how far the nearest player is
            SNetworkPlayer player = SNetworkPlayer.GetNearest(transform.position, range_max);
            if (player != null)
                return (transform.position - player.transform.position).magnitude;
            return range_max;
        }

        public float GetActiveRange()
        {
            if (_optimizer != null)
                return _optimizer.active_range;
            return 99999f;
        }

        public float GetRangePercent()
        {
            float arange = GetActiveRange();
            float prange = GetPlayerRange(arange);
            return Mathf.Clamp01(prange / arange); //Return from 0 to 1, represent how far from player 
        }

        public bool IsActive()
        {
            if(_optimizer != null)
                return _optimizer.IsActive() && gameObject.activeSelf;
            return gameObject.activeSelf;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetActive(bool active)
        {
            if (!IsServer && _optimizer != null)
                _optimizer.SetActive(active);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnValidate()
        {
            if (EditorAutoGenID)
                GenerateEditorID();
        }

        public ulong NetworkId => _networkID;
        public ulong PrefabId => _prefabID;
        public ulong OwnerId => _owner;

        public bool IsServer => TheNetwork.Get().IsServer;
        public bool IsClient => TheNetwork.Get().IsClient;
        public bool IsOwner => TheNetwork.Get().ClientID == _owner;

        public bool IsSpawned => _isSpawned;
        public bool IsReady => _isReady;
        public bool IsSceneObject => _isScene;
        public bool IsDestroyed => _isDestroying;

        public static List<SNetworkObject> GetAll() => NetObjects;

        public static void GenerateAllInScene()
        {
#if UNITY_EDITOR
            GenerateAll(FindObjectsOfType<SNetworkObject>());
#endif
        }

        public static void GenerateAll(SNetworkObject[] objs)
        {
#if UNITY_EDITOR
            EditorAutoGenID = false;
            foreach (SNetworkObject nobj in objs)
                nobj.GenerateEditorID();
            EditorAutoGenID = true;
#endif
        }

        public static void SpawnAll(SNetworkObject[] objs)
        {
            foreach (SNetworkObject nobj in objs)
                nobj.Spawn();
        }

        public static SNetworkObject Create(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (!TheNetwork.Get().IsServer)
                return null;

            GameObject obj = Instantiate(prefab, pos, rot);
            SNetworkObject nobj = obj.GetComponent<SNetworkObject>();
            nobj?.Spawn();
            return nobj;
        }
    }

    [Serializable]
    public struct SNetworkObjectRef : INetworkSerializable
    {
        public ulong _netID;

        public SNetworkObjectRef(SNetworkObject behaviour) =>
            _netID = behaviour != null ? behaviour.NetworkId : 0;

        public SNetworkObject Get() =>
            NetworkSpawner.Get().GetSpawnedObject(_netID);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
            serializer.SerializeValue(ref _netID);
    }

    public enum AutoSpawnType
    {
        SceneOnly = 0,  //Default, only scene object will be auto spawned, script Instantiated object need to call Spawn()
        Always = 10,    //All object will be autospawned in Start() or when the client is ready
        Never = 20,     //Never auto spawn, Spawn() must be called for each object, even the ones in the scene
    }
}
