﻿using GameCore.Infrastructure.Configs.Gameplay.Quests;
using GameCore.Infrastructure.Configs.Gameplay.QuestsItems;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.UI;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Systems.Quests
{
    public class QuestsManager : NetcodeBehaviour
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
            _questsConfig = gameplayConfigsProvider.GetConfig<QuestsConfigMeta>();

            var questsItemsConfig = gameplayConfigsProvider.GetConfig<QuestsItemsConfigMeta>();

            _questsStorage = new QuestsStorage();
            _questsController = new QuestsController(_questsStorage);
            _questsFactory = new QuestsFactory(_questsStorage, _questsConfig, questsItemsConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private IQuestsManagerDecorator _questsManagerDecorator;
        private IGameManagerDecorator _gameManagerDecorator;
        private IUIObserver _uiObserver;
        private QuestsConfigMeta _questsConfig;
        private QuestsController _questsController;
        private QuestsFactory _questsFactory;
        private QuestsStorage _questsStorage;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _questsManagerDecorator.OnCalculateRewardInnerEvent += CalculateReward;
            _questsManagerDecorator.OnSelectQuestInnerEvent += OnSelectQuest;
            _questsManagerDecorator.OnSubmitQuestItemInnerEvent += SubmitQuestItem;
            _questsManagerDecorator.OnCompleteQuestsInnerEvent += CompleteQuestsServerRpc;
            _questsManagerDecorator.OnDecreaseQuestsDaysInnerEvent += DecreaseQuestsDaysServerRpc;
            _questsManagerDecorator.OnResetQuestsInnerEvent += ResetQuests;
            _questsManagerDecorator.OnGetQuestsStorageInnerEvent += GetQuestsStorage;
            _questsManagerDecorator.OnGetActiveQuestsAmountInnerEvent += GetActiveQuestsAmount;
            _questsManagerDecorator.OnContainsItemInQuestsInnerEvent += ContainsItemInQuests;
            _questsManagerDecorator.OnContainsCompletedQuestsInnerEvent += ContainsCompletedQuests;
            _questsManagerDecorator.OnContainsExpiredQuestsInnerEvent += ContainsExpiredQuests;
            _questsManagerDecorator.OnContainsExpiredAndUncompletedQuestsInnerEvent +=
                ContainsExpiredAndUncompletedQuests;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _questsManagerDecorator.OnCalculateRewardInnerEvent -= CalculateReward;
            _questsManagerDecorator.OnSelectQuestInnerEvent -= OnSelectQuest;
            _questsManagerDecorator.OnSubmitQuestItemInnerEvent -= SubmitQuestItem;
            _questsManagerDecorator.OnCompleteQuestsInnerEvent -= CompleteQuestsServerRpc;
            _questsManagerDecorator.OnDecreaseQuestsDaysInnerEvent -= DecreaseQuestsDaysServerRpc;
            _questsManagerDecorator.OnResetQuestsInnerEvent -= ResetQuests;
            _questsManagerDecorator.OnGetQuestsStorageInnerEvent -= GetQuestsStorage;
            _questsManagerDecorator.OnGetActiveQuestsAmountInnerEvent -= GetActiveQuestsAmount;
            _questsManagerDecorator.OnContainsItemInQuestsInnerEvent -= ContainsItemInQuests;
            _questsManagerDecorator.OnContainsCompletedQuestsInnerEvent -= ContainsCompletedQuests;
            _questsManagerDecorator.OnContainsExpiredQuestsInnerEvent -= ContainsExpiredQuests;
            _questsManagerDecorator.OnContainsExpiredAndUncompletedQuestsInnerEvent -=
                ContainsExpiredAndUncompletedQuests;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly() => CreateQuests();

        protected override void InitNotOwner() => SynchronizeAwaitingQuestsDataServerRpc();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateQuests()
        {
            _questsFactory.Create();
            _questsManagerDecorator.AwaitingQuestsDataReceived();
        }

        private void SubmitQuestItem(int itemID) => SubmitQuestItemServerRpc(itemID);

        private void CalculateReward()
        {
            int reward = _questsController.CalculateReward();

            _uiObserver.ShowRewardMenu(reward);
            _questsStorage.ClearCompletedQuests();

            if (IsServerOnly)
                _gameManagerDecorator.AddPlayersGold(reward);
        }

        private void ResetQuests()
        {
            _questsStorage.ClearAll();
            _questsManagerDecorator.ActiveQuestsDataReceived();
            CreateQuests();
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

        private bool ContainsItemInQuests(int itemID) =>
            _questsController.ContainsItemInQuests(itemID);

        private bool ContainsCompletedQuests() =>
            _questsController.ContainsCompletedQuests();

        private bool ContainsExpiredQuests() =>
            _questsController.ContainsExpiredQuests();

        private bool ContainsExpiredAndUncompletedQuests() =>
            _questsController.ContainsExpiredAndUncompletedQuests();

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SynchronizeAwaitingQuestsDataServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderClientID = serverRpcParams.Receive.SenderClientId;

            QuestRuntimeDataContainer[] questsRuntimeDataContainers =
                _questsStorage.GetAwaitingQuestsRuntimeDataContainers();

            SynchronizeAwaitingQuestsDataClientRpc(questsRuntimeDataContainers, senderClientID);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SelectQuestServerRpc(int questID) => SelectQuestClientRpc(questID);

        [ServerRpc(RequireOwnership = false)]
        private void SubmitQuestItemServerRpc(int itemID) => SubmitQuestItemClientRpc(itemID);

        [ServerRpc]
        private void DecreaseQuestsDaysServerRpc() => DecreaseQuestsDaysClientRpc();

        [ServerRpc(RequireOwnership = false)]
        private void CompleteQuestsServerRpc() => CompleteQuestsClientRpc();

        [ClientRpc]
        private void SynchronizeAwaitingQuestsDataClientRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers,
            ulong requestedClientId)
        {
            if (IsServerOnly)
                return;

            if (NetworkHorror.ClientID != requestedClientId)
                return;

            _questsStorage.UpdateAwaitingQuestsData(questsRuntimeDataContainers);
            _questsManagerDecorator.AwaitingQuestsDataReceived();
        }

        [ClientRpc]
        private void SelectQuestClientRpc(int questID)
        {
            _questsController.SelectQuest(questID);
            _questsManagerDecorator.ActiveQuestsDataReceived();
            _questsManagerDecorator.AwaitingQuestsDataReceived();
        }

        [ClientRpc]
        private void SubmitQuestItemClientRpc(int itemID)
        {
            _questsController.SubmitQuestItem(itemID);
            _questsManagerDecorator.UpdateQuestsProgress();
        }

        [ClientRpc]
        private void DecreaseQuestsDaysClientRpc()
        {
            _questsController.DecreaseDays();
            _questsManagerDecorator.UpdateQuestsProgress();
        }

        [ClientRpc]
        private void CompleteQuestsClientRpc()
        {
            _questsController.CompleteQuests();
            _questsManagerDecorator.ActiveQuestsDataReceived();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectQuest(int questID)
        {
            if (!CanGetNewQuest())
                return;

            SelectQuestServerRpc(questID);
        }
    }
}