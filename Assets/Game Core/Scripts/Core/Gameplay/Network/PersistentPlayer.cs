using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.Network.SessionManagement;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    /// <summary>
    /// NetworkBehaviour that represents a player connection and is the "Default Player Prefab" inside Netcode for
    /// GameObjects' (Netcode) NetworkManager. This NetworkBehaviour will contain several other NetworkBehaviours that
    /// should persist throughout the duration of this connection, meaning it will persist between scenes.
    /// </summary>
    /// <remarks>
    /// It is not necessary to explicitly mark this as a DontDestroyOnLoad object as Netcode will handle migrating this
    /// Player object between scene loads.
    /// </remarks>
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            name = "Persistent Player #" + OwnerClientId;

            if (!IsServer)
                return;

            SessionPlayerData? sessionPlayerData =
                SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);

            if (!sessionPlayerData.HasValue)
                return;

            SessionPlayerData playerData = sessionPlayerData.Value;
            //m_NetworkNameState.Name.Value = playerData.PlayerName;

            if (playerData.HasCharacterSpawned)
            {
                //m_NetworkAvatarGuidState.AvatarGuid.Value = playerData.AvatarNetworkGuid;
            }
            else
            {
                //m_NetworkAvatarGuidState.SetRandomAvatar();
                //playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            RemovePersistentPlayer();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RemovePersistentPlayer()
        {
            if (!IsServer)
                return;

            SessionPlayerData? sessionPlayerData =
                SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);

            if (!sessionPlayerData.HasValue)
                return;

            SessionPlayerData playerData = sessionPlayerData.Value;
            //playerData.PlayerName = m_NetworkNameState.Name.Value;
            //playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;

            SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
        }
    }
}