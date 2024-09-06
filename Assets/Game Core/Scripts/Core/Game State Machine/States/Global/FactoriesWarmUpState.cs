using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Gameplay.Factories.Monsters;
using GameCore.Gameplay.Network.Utilities;

namespace GameCore.StateMachine
{
    public class FactoriesWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FactoriesWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenuFactory menuFactory,
            IEntitiesFactory entitiesFactory,
            IMonstersFactory monstersFactory,
            GlobalNetworkPrefabsRegistrar globalNetworkPrefabsRegistrar
        )
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;
            _entitiesFactory = entitiesFactory;
            _monstersFactory = monstersFactory;
            _globalNetworkPrefabsRegistrar = globalNetworkPrefabsRegistrar;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;
        private readonly IEntitiesFactory _entitiesFactory;
        private readonly IMonstersFactory _monstersFactory;
        private readonly GlobalNetworkPrefabsRegistrar _globalNetworkPrefabsRegistrar;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await WarmUpFactories();
            await _globalNetworkPrefabsRegistrar.RegisterPrefabs();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask WarmUpFactories()
        {
            await _menuFactory.WarmUp();
            await _entitiesFactory.WarmUp();
            await _monstersFactory.WarmUp();
        }
        
        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}