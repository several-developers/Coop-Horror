using System;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Network;
using GameCore.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
#warning Это какой-то пиздец. Поправить в будущем, когда умнее буду.

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
        
        public async UniTaskVoid PlaySound(TSFXType sfxType, bool onlyLocal = false, float delay = 0f)
        {
            if (delay > 0.0f)
            {
                int delayInMilliseconds = delay.ConvertToMilliseconds();

                bool isCanceled = await UniTask
                    .DelayFrame(delayInMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;
            }

            if (onlyLocal)
                PlaySoundLocal(sfxType);
            else
                PlaySoundRpc(sfxType);
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

        [Rpc(target: SendTo.Everyone)]
        private void PlaySoundRpc(TSFXType sfxType) => PlaySoundLocal(sfxType);
        
        [ServerRpc(RequireOwnership = false)]
        private void StopSoundServerRPC(TSFXType sfxType, ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            StopSoundClientRPC(sfxType, senderClientID);
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

        public async UniTaskVoid PlaySound(TSFXType sfxType, bool onlyLocal = false, float delay = 0f)
        {
            if (delay > 0.0f)
            {
                int delayInMilliseconds = delay.ConvertToMilliseconds();

                bool isCanceled = await UniTask
                    .DelayFrame(delayInMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;
            }

            if (onlyLocal)
                PlaySoundLocal(sfxType);
            else
                PlaySoundRpc(sfxType);
        }

        public void StopSound(TSFXType sfxType) => StopSoundRpc(sfxType);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void PlaySoundRpc(TSFXType sfxType) => PlaySoundLocal(sfxType);

        [Rpc(target: SendTo.Everyone)]
        private void StopSoundRpc(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);
    }

    public abstract class SoundProducerNavmeshMonsterEntity<TSFXType> : NavmeshMonsterEntityBase,
        ISoundProducer<TSFXType> where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };

        protected SoundReproducerBase<TSFXType> SoundReproducer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid PlaySound(TSFXType sfxType, float delay = 0f, bool onlyLocal = false)
        {
            if (delay > 0.0f)
            {
                int delayInMilliseconds = delay.ConvertToMilliseconds();

                bool isCanceled = await UniTask
                    .DelayFrame(delayInMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;
            }

            if (onlyLocal)
                PlaySoundLocal(sfxType);
            else
                PlaySoundRpc(sfxType);
        }

        public void StopSound(TSFXType sfxType) => StopSoundRpc(sfxType);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void PlaySoundRpc(TSFXType sfxType) => PlaySoundLocal(sfxType);

        [Rpc(target: SendTo.Everyone)]
        private void StopSoundRpc(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);
    }

    public class SoundProducerMonoBehaviour<TSFXType> : MonoBehaviour, ISoundProducer<TSFXType> where TSFXType : Enum
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<TSFXType> OnPlaySoundEvent = delegate { };
        public event Action<TSFXType> OnStopSoundEvent = delegate { };

        protected SoundReproducerBase<TSFXType> SoundReproducer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid PlaySound(TSFXType sfxType, bool onlyLocal = false, float delay = 0f)
        {
            if (delay > 0.0f)
            {
                int delayInMilliseconds = delay.ConvertToMilliseconds();

                bool isCanceled = await UniTask
                    .DelayFrame(delayInMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy())
                    .SuppressCancellationThrow();

                if (isCanceled)
                    return;
            }

            if (onlyLocal)
                PlaySoundLocal(sfxType);
            else
                PlaySoundRpc(sfxType);
        }

        public void StopSound(TSFXType sfxType) => StopSoundRpc(sfxType);

        public Transform GetTransform() => transform;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlaySoundLocal(TSFXType sfxType) =>
            OnPlaySoundEvent.Invoke(sfxType);

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void PlaySoundRpc(TSFXType sfxType) => PlaySoundLocal(sfxType);
        
        [Rpc(target: SendTo.Everyone)]
        private void StopSoundRpc(TSFXType sfxType) =>
            OnStopSoundEvent.Invoke(sfxType);
    }
}