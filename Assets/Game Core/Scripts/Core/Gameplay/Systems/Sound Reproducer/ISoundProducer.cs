using System;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
#warning Это какой-то пиздец. Поправить в будущем, когда умнее буду
    
    // Проблема в RPC методах. Для них нужен NetworkBehaviour, т.е. я не могу засунуть класс в переменную и 
    // использовать её везде где нужно. Вместо этого приходится дублировать ебучие классы для корректного
    // наследования. Можно попробовать заменить на кастомные сообщения для синхронизации, должно помочь.
    
    // [GenerateSerializationForType(typeof(SFXType))]
    public interface ISoundProducer<out TSFXType> where TSFXType : Enum
    {
        event Action<TSFXType> OnPlaySoundEvent;
        event Action<TSFXType> OnStopSoundEvent;
        Transform GetTransform();
    }
    
    public class SoundProducerEntity<TSFXType> : Entity, ISoundProducer<TSFXType> where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };
        
        protected SoundReproducerBase<TSFXType> SoundReproducer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TSFXType sfxType, bool onlyLocal = false)
        {
            PlaySoundLocal(sfxType);

            if (onlyLocal)
                return;

            PlaySoundServerRPC(sfxType);
        }

        public void StopSound(TSFXType sfxType)
        {
            StopSoundLocal(sfxType);
            StopSoundServerRPC(sfxType);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        private void StopSoundLocal(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);

        private static bool IsClientIDMatches(ulong targetClientID) =>
            NetworkHorror.ClientID == targetClientID;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            PlaySoundClientRPC(sfxType, senderClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopSoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            StopSoundClientRPC(sfxType, senderClientID);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(TSFXType sfxType, ulong senderClientID)
        {
            bool isClientIDMatches = IsClientIDMatches(senderClientID);

            // Don't reproduce sound twice on sender.
            if (isClientIDMatches)
                return;

            PlaySoundLocal(sfxType);
        }

        [ClientRpc]
        private void StopSoundClientRPC(TSFXType sfxTypeIndex, ulong senderClientID)
        {
            bool isClientIDMatches = senderClientID == NetworkHorror.ClientID;

            if (isClientIDMatches)
                return;

            StopSoundLocal(sfxTypeIndex);
        }
    }

    public abstract class SoundProducerMonsterEntity<TSFXType> : MonsterEntityBase, ISoundProducer<TSFXType>
        where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };
        
        protected SoundReproducerBase<TSFXType> SoundReproducer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TSFXType sfxType, bool onlyLocal = false)
        {
            PlaySoundLocal(sfxType);

            if (onlyLocal)
                return;

            PlaySoundServerRPC(sfxType);
        }

        public void StopSound(TSFXType sfxType)
        {
            StopSoundLocal(sfxType);
            StopSoundServerRPC(sfxType);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        private void StopSoundLocal(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            PlaySoundClientRPC(sfxType, senderClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopSoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            StopSoundClientRPC(sfxType, senderClientID);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(TSFXType sfxType, ulong senderClientID)
        {
            bool isClientIDMatches = IsClientIDMatches(senderClientID);

            // Don't reproduce sound twice on sender.
            if (isClientIDMatches)
                return;

            PlaySoundLocal(sfxType);
        }

        [ClientRpc]
        private void StopSoundClientRPC(TSFXType sfxTypeIndex, ulong senderClientID)
        {
            bool isClientIDMatches = senderClientID == NetworkHorror.ClientID;

            if (isClientIDMatches)
                return;

            StopSoundLocal(sfxTypeIndex);
        }
    }

    public abstract class SoundProducerNavmeshMonsterEntity<TSFXType> : NavmeshMonsterEntityBase,
        ISoundProducer<TSFXType> where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };

        protected SoundReproducerBase<TSFXType> SoundReproducer;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TSFXType sfxType, bool onlyLocal = false)
        {
            PlaySoundLocal(sfxType);

            if (onlyLocal)
                return;

            PlaySoundServerRPC(sfxType);
        }

        public void StopSound(TSFXType sfxType)
        {
            StopSoundLocal(sfxType);
            StopSoundServerRPC(sfxType);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        private void StopSoundLocal(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            PlaySoundClientRPC(sfxType, senderClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopSoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            StopSoundClientRPC(sfxType, senderClientID);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(TSFXType sfxType, ulong senderClientID)
        {
            bool isClientIDMatches = IsClientIDMatches(senderClientID);

            // Don't reproduce sound twice on sender.
            if (isClientIDMatches)
                return;

            PlaySoundLocal(sfxType);
        }

        [ClientRpc]
        private void StopSoundClientRPC(TSFXType sfxTypeIndex, ulong senderClientID)
        {
            bool isClientIDMatches = senderClientID == NetworkHorror.ClientID;

            if (isClientIDMatches)
                return;

            StopSoundLocal(sfxTypeIndex);
        }
    }

    public class SoundProducerMonoBehaviour<TSFXType> : MonoBehaviour, ISoundProducer<TSFXType> where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };
        
        protected SoundReproducerBase<TSFXType> SoundReproducer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(TSFXType sfxType, bool onlyLocal = false)
        {
            PlaySoundLocal(sfxType);

            if (onlyLocal)
                return;

            PlaySoundServerRPC(sfxType);
        }

        public void StopSound(TSFXType sfxType)
        {
            StopSoundLocal(sfxType);
            StopSoundServerRPC(sfxType);
        }

        public Transform GetTransform() => transform;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        private void StopSoundLocal(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);

        private static bool IsClientIDMatches(ulong targetClientID) =>
            NetworkHorror.ClientID == targetClientID;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            PlaySoundClientRPC(sfxType, senderClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StopSoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            StopSoundClientRPC(sfxType, senderClientID);
        }

        [ClientRpc]
        private void PlaySoundClientRPC(TSFXType sfxType, ulong senderClientID)
        {
            bool isClientIDMatches = IsClientIDMatches(senderClientID);

            // Don't reproduce sound twice on sender.
            if (isClientIDMatches)
                return;

            PlaySoundLocal(sfxType);
        }

        [ClientRpc]
        private void StopSoundClientRPC(TSFXType sfxTypeIndex, ulong senderClientID)
        {
            bool isClientIDMatches = senderClientID == NetworkHorror.ClientID;

            if (isClientIDMatches)
                return;

            StopSoundLocal(sfxTypeIndex);
        }
    }
}