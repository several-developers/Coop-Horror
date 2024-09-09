using GameCore.Infrastructure.Providers.Global;

namespace GameCore.StateMachine
{
    public class PrepareMainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareMainMenuState(IGameStateMachine gameStateMachine, IAssetsProvider assetsProvider)
        {
            _gameStateMachine = gameStateMachine;
            _assetsProvider = assetsProvider;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAssetsProvider _assetsProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            //CleanUpAddressables();
            EnterSignInState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CleanUpAddressables() =>
            _assetsProvider.Cleanup();

        private void EnterSignInState() =>
            _gameStateMachine.ChangeState<SignInState>();
    }
}