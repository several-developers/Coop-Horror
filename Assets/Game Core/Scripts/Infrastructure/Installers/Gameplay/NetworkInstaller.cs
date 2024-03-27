using GameCore.Configs.Gameplay.PrefabsList;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class NetworkInstaller : MonoInstaller
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider) =>
            _gameplayConfigsProvider = gameplayConfigsProvider;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameplayConfigsProvider _gameplayConfigsProvider;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindElevatorsManager();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindElevatorsManager()
        {
            PrefabsListConfigMeta prefabsListConfig = _gameplayConfigsProvider.GetPrefabsListConfig();
            ElevatorsManager elevatorsManagerPrefab = prefabsListConfig.ElevatorsManager;
            ElevatorsManager elevatorsManagerInstance = Instantiate(elevatorsManagerPrefab);
            
            Container
                .Bind<IElevatorsManager>()
                .To<ElevatorsManager>()
                .FromInstance(elevatorsManagerInstance)
                .AsSingle()
                .NonLazy();
        }
    }
}