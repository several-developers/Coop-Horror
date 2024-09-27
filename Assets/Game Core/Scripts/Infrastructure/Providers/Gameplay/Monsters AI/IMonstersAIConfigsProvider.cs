using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public interface IMonstersAIConfigsProvider
    {
        TMonsterConfigType GetConfig<TMonsterConfigType>() where TMonsterConfigType : MonsterAIConfigMeta;
        bool TryGetMonsterAIConfig(MonsterType monsterType, out MonsterAIConfigMeta monsterAIConfig);
    }
}