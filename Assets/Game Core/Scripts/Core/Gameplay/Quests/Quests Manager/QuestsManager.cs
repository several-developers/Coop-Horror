using GameCore.Configs.Gameplay.Quests;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManager : NetworkBehaviour, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IQuestsManagerDecorator questsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;
            QuestsConfigMeta questsConfig = gameplayConfigsProvider.GetQuestsConfig();

            _questsStorage = new QuestsStorage();
            _questsFactory = new QuestsFactory(_questsStorage, questsConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private QuestsFactory _questsFactory;
        private QuestsStorage _questsStorage;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _questsManagerDecorator.OnGetQuestsStorageEvent += GetQuestsStorage;

        public override void OnDestroy()
        {
            _questsManagerDecorator.OnGetQuestsStorageEvent -= GetQuestsStorage;

            base.OnDestroy();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InitServerAndClient()
        {
        }

        public void InitServer()
        {
            if (!IsOwner)
                return;

            _questsFactory.Create();
            _questsManagerDecorator.QuestsDataReceived();
        }

        public void InitClient()
        {
            if (IsOwner)
                return;

            RequestQuestsDataServerRpc();
        }

        public void DespawnServerAndClient()
        {
        }

        public void DespawnServer()
        {
            if (!IsOwner)
                return;
        }

        public void DespawnClient()
        {
            if (IsOwner)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SynchronizeQuestsData(ulong requestedClientId)
        {
            QuestRuntimeDataContainer[] questsRuntimeDataContainers = _questsStorage.GetQuestsRuntimeDataContainers();
            SynchronizeQuestsDataServerRpc(questsRuntimeDataContainers, requestedClientId);
        }

        private QuestsStorage GetQuestsStorage() => _questsStorage;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc]
        private void SynchronizeQuestsDataServerRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers,
            ulong requestedClientId)
        {
            SynchronizeQuestsDataClientRpc(questsRuntimeDataContainers, requestedClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestQuestsDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            RequestQuestsDataClientRpc(clientId);
        }

        [ClientRpc]
        private void SynchronizeQuestsDataClientRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers,
            ulong requestedClientId)
        {
            if (IsOwner)
                return;

            if (NetworkHorror.ClientID != requestedClientId)
                return;

            _questsStorage.UpdateQuestsData(questsRuntimeDataContainers);
            _questsManagerDecorator.QuestsDataReceived();
        }

        [ClientRpc]
        private void RequestQuestsDataClientRpc(ulong requestedClientId)
        {
            if (!IsOwner)
                return;
            
            SynchronizeQuestsData(requestedClientId);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }
    }
}