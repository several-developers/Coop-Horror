using GameCore.Configs.Gameplay.Enemies;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public interface IMonstersAIConfigsProvider
    {
        GoodClownAIConfigMeta GetGoodClownAIConfig();
        EvilClownAIConfigMeta GetEvilClownAIConfig();
        BeetleAIConfigMeta GetBeetleAIConfig();
    }
}