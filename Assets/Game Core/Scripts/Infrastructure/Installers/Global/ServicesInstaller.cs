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
        private ScenesLoaderService2 _scenesLoaderService;
        
        // FIELDS: --------------------------------------------------------------------------------

        private DataInstaller _dataInstaller;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindDataServices();
            BindSaveLoadService();
            BindScenesLoaderService();
            BindScenesLoaderService2();
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
                .BindInterfacesTo<ScenesLoaderService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindScenesLoaderService2()
        {
            Container
                .Bind<IScenesLoaderService2>()
                .To<ScenesLoaderService2>()
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