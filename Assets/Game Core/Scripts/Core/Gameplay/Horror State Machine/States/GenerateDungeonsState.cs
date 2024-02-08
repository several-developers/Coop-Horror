using DunGen;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.Dungeons;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GenerateDungeonsState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GenerateDungeonsState(IHorrorStateMachine horrorStateMachine, IDungeonsObserver dungeonsObserver)
        {
            _horrorStateMachine = horrorStateMachine;
            _dungeonsObserver = dungeonsObserver;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IDungeonsObserver _dungeonsObserver;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            SendGenerateDungeons();

            _dungeonsObserver.OnDungeonsGenerationCompletedEvent += OnDungeonsGenerationCompleted;
        }
        
        public void Exit() =>
            _dungeonsObserver.OnDungeonsGenerationCompletedEvent -= OnDungeonsGenerationCompleted;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();

        private static void SendGenerateDungeons()
        {
            RandomStream randomStream = new();
            int seedOne = GenerateRandomSeed(randomStream);
            int seedTwo = GenerateRandomSeed(randomStream);
            int seedThree = GenerateRandomSeed(randomStream);
            DungeonsSeedData data = new(seedOne, seedTwo, seedThree);
            
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.SendGenerateDungeons(data);
        }
        
        private static int GenerateRandomSeed(RandomStream randomStream) =>
            randomStream.Next();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonsGenerationCompleted() => EnterGameLoopState();
    }
}