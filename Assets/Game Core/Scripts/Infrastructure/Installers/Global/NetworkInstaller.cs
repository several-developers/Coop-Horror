using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Installers.Global
{
    public class NetworkInstaller : MonoInstaller
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private NetcodeHooks _netcodeHooksPrefab;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindNetworkHorrorDecorator();
            BindNetcodeHooks();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void BindNetworkHorrorDecorator()
        {
            Container
                .BindInterfacesTo<NetworkHorrorDecorator>()
                .AsSingle();
        }

        private void BindNetcodeHooks()
        {
            Container
                .Bind<INetcodeHooks>()
                .To<NetcodeHooks>()
                .FromComponentInNewPrefab(_netcodeHooksPrefab)
                .AsSingle()
                .NonLazy();
        }
    }
}