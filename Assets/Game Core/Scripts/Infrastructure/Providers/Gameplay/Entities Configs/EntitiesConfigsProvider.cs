using GameCore.Infrastructure.Configs.Gameplay.Entities;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesConfigs
{
    public class EntitiesConfigsProvider : IEntitiesConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesConfigsProvider(IGameplayConfigsProvider gameplayConfigsProvider) =>
            _gameplayConfigsProvider = gameplayConfigsProvider;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameplayConfigsProvider _gameplayConfigsProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public TEntityConfigType GetConfig<TEntityConfigType>() where TEntityConfigType : EntityConfigMeta =>
            _gameplayConfigsProvider.GetConfig<TEntityConfigType>();
    }
}