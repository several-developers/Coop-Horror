using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public interface IMonstersAIConfigsProvider
    {
        bool TryGetMonsterAIConfig(MonsterType monsterType, out MonsterAIConfigMeta monsterAIConfig);
        GoodClownAIConfigMeta GetGoodClownAIConfig();
        EvilClownAIConfigMeta GetEvilClownAIConfig();
        BeetleAIConfigMeta GetBeetleAIConfig();
        BlindCreatureAIConfigMeta GetBlindCreatureAIConfig();
    }
}