using GameCore.Gameplay.Levels.Elevator;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class NetworkInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindElevatorsManagerDecorator();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindElevatorsManagerDecorator()
        {
            Container
                .BindInterfacesTo<ElevatorsManagerDecorator>()
                .AsSingle()
                .NonLazy();
        }
    }
}