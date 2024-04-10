using GameCore.Configs.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Unity.Netcode;
using Zenject;

namespace GameCore.Gameplay.Quests
{
    public class QuestsManager : NetworkBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            QuestsConfigMeta questsConfig = gameplayConfigsProvider.GetQuestsConfig();
            
            _questsStorage = new QuestsStorage();
            _questsFactory = new QuestsFactory(_questsStorage, questsConfig);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private QuestsFactory _questsFactory;
        private QuestsStorage _questsStorage;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SynchronizeQuestsData()
        {
            QuestRuntimeDataContainer[] questsRuntimeDataContainers = _questsStorage.GetQuestsRuntimeDataContainers();
            SynchronizeQuestsDataServerRpc(questsRuntimeDataContainers);
        }
        
        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc]
        private void SynchronizeQuestsDataServerRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers) =>
            SynchronizeQuestsDataClientRpc(questsRuntimeDataContainers);

        [ClientRpc]
        private void SynchronizeQuestsDataClientRpc(QuestRuntimeDataContainer[] questsRuntimeDataContainers)
        {
            if (IsOwner)
                return;
            
            _questsStorage.UpdateQuestsData(questsRuntimeDataContainers);
        }
    }
}