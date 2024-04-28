using GameCore.Configs.Gameplay.Quests;
using GameCore.Configs.Gameplay.QuestsItems;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.UI;
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
            IGameManagerDecorator gameManagerDecorator, IUIObserver uiObserver,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _questsManagerDecorator = questsManagerDecorator;
            _gameManagerDecorator = gameManagerDecorator;
            _uiObserver = uiObserver;
            _questsConfig = gameplayConfigsProvider.GetQuestsConfig();

            QuestsItemsConfigMeta questsItemsConfig = gameplayConfigsProvider.GetQuestsItemsConfig();

            _questsStorage = new QuestsStorage();
            _questsFactory = new QuestsFactory(_questsStorage, _questsConfig, questsItemsConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private IGameManagerDecorator _gameManagerDecorator;
        private IUIObserver _uiObserver;
        private QuestsConfigMeta _questsConfig;
        private QuestsFactory _questsFactory;
        private QuestsStorage _questsStorage;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _questsManagerDecorator.OnSelectQuestInnerEvent += OnSelectQuest;
            _questsManagerDecorator.OnSubmitQuestItemInnerEvent += SubmitQuestItem;
            _questsManagerDecorator.OnCompleteQuestsInnerEvent += CompleteQuests;
            _questsManagerDecorator.OnGetQuestsStorageInnerEvent += GetQuestsStorage;
            _questsManagerDecorator.OnGetActiveQuestsAmountInnerEvent += GetActiveQuestsAmount;
            _questsManagerDecorator.OnContainsCompletedQuestsInnerEvent += ContainsCompletedQuests;
            _questsManagerDecorator.OnContainsExpiredQuestsInnerEvent += ContainsExpiredQuests;
            _questsManagerDecorator.OnContainsExpiredAndUncompletedQuestsInnerEvent +=
                ContainsExpiredAndUncompletedQuests;

            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
        }

        public override void OnDestroy()
        {
            _questsManagerDecorator.OnSelectQuestInnerEvent -= OnSelectQuest;
            _questsManagerDecorator.OnSubmitQuestItemInnerEvent -= SubmitQuestItem;
            _questsManagerDecorator.OnCompleteQuestsInnerEvent -= CompleteQuests;
            _questsManagerDecorator.OnGetQuestsStorageInnerEvent -= GetQuestsStorage;
            _questsManagerDecorator.OnGetActiveQuestsAmountInnerEvent -= GetActiveQuestsAmount;
            _questsManagerDecorator.OnContainsCompletedQuestsInnerEvent -= ContainsCompletedQuests;
            _questsManagerDecorator.OnContainsExpiredQuestsInnerEvent -= ContainsExpiredQuests;
            _questsManagerDecorator.OnContainsExpiredAndUncompletedQuestsInnerEvent -=
                ContainsExpiredAndUncompletedQuests;

            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

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
            QuestRuntimeDataContainer[] questsRuntimeDataContainers =
                _questsStorage.GetAwaitingQuestsRuntimeDataContainers();

            SynchronizeAwaitingQuestsDataServerRpc(questsRuntimeDataContainers, requestedClientId);
        }

        private void SubmitQuestItem(int itemID) => SubmitQuestItemServerRpc(itemID);

        private void CompleteQuests()
        {
            _questsStorage.CompleteQuests();
            _questsManagerDecorator.ActiveQuestsDataReceived();
        }

        private void CalculateReward()
        {
            int reward = _questsStorage.CalculateReward();
            Debug.LogWarning("Reward: " + reward);

            _uiObserver.ShowRewardMenu(reward);
            _questsStorage.ClearCompletedQuests();

            if (IsOwner)
                _gameManagerDecorator.AddPlayersGold(reward);
        }

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.ArrivedAtTheRoad:
                    _questsStorage.DecreaseDays();
                    _questsManagerDecorator.UpdateQuestsProgress();
                    break;
                
                case GameState.QuestsRewarding:
                    CalculateReward();
                    _questsFactory.Create();
                    _questsManagerDecorator.AwaitingQuestsDataReceived();
                    _gameManagerDecorator.ChangeGameState(GameState.ReadyToLeaveTheRoad, ownerOnly: true);
                    break;
            }
        }

        private QuestsStorage GetQuestsStorage() => _questsStorage;

        private int GetActiveQuestsAmount() =>
            _questsStorage.GetActiveQuestsAmount();

        private bool CanGetNewQuest()
        {
            int activeQuestsAmount = GetActiveQuestsAmount();
            bool canGetNewQuest = activeQuestsAmount < _questsConfig.MaxActiveQuests;
            return canGetNewQuest;
        }

        private bool ContainsCompletedQuests() =>
            _questsStorage.ContainsCompletedQuests();

        private bool ContainsExpiredQuests() =>
            _questsStorage.ContainsExpiredQuests();

        private bool ContainsExpiredAndUncompletedQuests() =>
            _questsStorage.ContainsExpiredAndUncompletedQuests();

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

        [ServerRpc(RequireOwnership = false)]
        private void SubmitQuestItemServerRpc(int itemID) => SubmitQuestItemClientRpc(itemID);

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

        [ClientRpc]
        private void SubmitQuestItemClientRpc(int itemID)
        {
            _questsStorage.SubmitQuestItem(itemID);
            _questsManagerDecorator.UpdateQuestsProgress();
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

        private void OnSelectQuest(int questID)
        {
            if (!CanGetNewQuest())
                return;

            SelectQuestServerRpc(questID);
        }

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}