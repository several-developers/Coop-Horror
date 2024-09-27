using GameCore.Infrastructure.Services.Global;
using GameCore.Infrastructure.Services.Global.Rewards;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public partial class ServicesInstaller : MonoInstaller
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ScenesLoaderService _scenesLoaderService;
        
        // FIELDS: --------------------------------------------------------------------------------

        private DataInstaller _dataInstaller;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindDataServices();
            BindSaveLoadService();
            BindScenesLoaderService();
            BindRewardsService();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindDataServices()
        {
            _dataInstaller = new DataInstaller(Container);
            _dataInstaller.BindDataServices();
        }

        private void BindSaveLoadService()
        {
            Container
                .BindInterfacesTo<SaveLoadService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindScenesLoaderService()
        {
            Container
                .Bind<IScenesLoaderService>()
                .To<ScenesLoaderService>()
                .FromComponentInNewPrefab(_scenesLoaderService)
                .AsSingle()
                .NonLazy();
        }

        private void BindRewardsService()
        {
            Container
                .BindInterfacesTo<RewardsService>()
                .AsSingle();
        }
    }
}