using GameCore.Infrastructure.StateMachine;
using GameCore.StateMachine;
using UnityEngine;
using Zenject;

namespace GameCore.Infrastructure.Lifecycle
{
    // ----------------------------------------------------------------------
    //      - Находится в Resources/Project Context.prefab
    //      - Игра корректно работает при запуске из любой сцены.
    // ----------------------------------------------------------------------
    public class GameBootstrap : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine) =>
            _gameStateMachine = gameStateMachine;

        // FIELDS: --------------------------------------------------------------------------------

        private const int TargetFrameRate = 144;

        private IGameStateMachine _gameStateMachine;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            SetApplicationFrameRate();
            EnterBootstrapState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterBootstrapState() =>
            _gameStateMachine.ChangeState<BootstrapState>();
        
        private static void SetApplicationFrameRate() =>
            Application.targetFrameRate = TargetFrameRate;
    }
}