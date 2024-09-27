using GameCore.Gameplay.Storages.Entities;
using Zenject;

namespace GameCore.Infrastructure.Installers.Gameplay
{
    public class StoragesInstaller : MonoInstaller
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            BindEntitiesStorage();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void BindEntitiesStorage()
        {
            Container
                .BindInterfacesTo<EntitiesStorage>()
                .AsSingle();
        }
    }
}