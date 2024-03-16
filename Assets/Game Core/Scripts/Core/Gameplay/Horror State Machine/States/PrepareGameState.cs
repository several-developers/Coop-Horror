using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories.Player;
using UnityEngine;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class PrepareGameState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameState(IHorrorStateMachine horrorStateMachine, IPlayerFactory playerFactory)
        {
            _horrorStateMachine = horrorStateMachine;
            _playerFactory = playerFactory;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IPlayerFactory _playerFactory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreatePlayer();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreatePlayer()
        {
            Vector3 spawnPosition = new(0, 2, 1000);
            //PlayerEntity playerInstance = _playerFactory.Create(spawnPosition);
        }
        
        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}