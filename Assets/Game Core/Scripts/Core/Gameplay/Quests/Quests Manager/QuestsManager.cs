using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
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
            QuestsItemsConfigMeta questsItemsConfig = gameplayConfigsProvider.GetQuestsItemsConfig();

            _questsStorage = new QuestsStorage();
            _questsFactory = new QuestsFactory(_questsStorage, questsConfig, questsItemsConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private QuestsFactory _questsFactory;
        private QuestsStorage _questsStorage;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _questsManagerDecorator.OnSelectQuestEvent += OnSelectQuest;
            _questsManagerDecorator.OnGetQuestsStorageEvent += GetQuestsStorage;
        }

        public override void OnDestroy()
        {
            _questsManagerDecorator.OnSelectQuestEvent -= OnSelectQuest;
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
            _questsManagerDecorator.AwaitingQuestsDataReceived();
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
        
        private void SynchronizeAwaitingQuestsData(ulong requestedClientId)
        {
            QuestRuntimeDataContainer[] questsRuntimeDataContainers = _questsStorage.GetAwaitingQuestsRuntimeDataContainers();
            SynchronizeAwaitingQuestsDataServerRpc(questsRuntimeDataContainers, requestedClientId);
        }

        private QuestsStorage GetQuestsStorage() => _questsStorage;

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc]
        private void SynchronizeAwaitingQuestsDataServerRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers,
            ulong requestedClientId)
        {
            SynchronizeAwaitingQuestsDataClientRpc(questsRuntimeDataContainers, requestedClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestQuestsDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            RequestQuestsDataClientRpc(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectQuestServerRpc(int questID) => SelectQuestClientRpc(questID);

        [ClientRpc]
        private void SynchronizeAwaitingQuestsDataClientRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers,
            ulong requestedClientId)
        {
            if (IsOwner)
                return;

            if (NetworkHorror.ClientID != requestedClientId)
                return;

            _questsStorage.UpdateAwaitingQuestsData(questsRuntimeDataContainers);
            _questsManagerDecorator.AwaitingQuestsDataReceived();
        }

        [ClientRpc]
        private void RequestQuestsDataClientRpc(ulong requestedClientId)
        {
            if (!IsOwner)
                return;
            
            SynchronizeAwaitingQuestsData(requestedClientId);
        }

        [ClientRpc]
        private void SelectQuestClientRpc(int questID)
        {
            _questsStorage.SelectQuest(questID);
            _questsManagerDecorator.ActiveQuestsDataReceived();
            _questsManagerDecorator.AwaitingQuestsDataReceived();
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

        private void OnSelectQuest(int questID) => SelectQuestServerRpc(questID);
    }
}