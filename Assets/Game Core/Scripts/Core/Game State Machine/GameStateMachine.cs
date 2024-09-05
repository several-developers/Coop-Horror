using System;
using GameCore.Infrastructure.StateMachine;
using GameCore.Observers.Global.StateMachine;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.StateMachine
{
    public class GameStateMachine : IGameStateMachine
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameStateMachine(IGameStateMachineObserver gameStateMachineObserver)
        {
            _gameStateMachineObserver = gameStateMachineObserver;
            _stateMachine = new StateMachineBase();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly StateMachineBase _stateMachine;
        private readonly IGameStateMachineObserver _gameStateMachineObserver;

        private IState _currentState;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddState(IState state) =>
            _stateMachine.AddStateWithRemove(state);

        public void ChangeState<T>() where T : IState
        {
            LogStateChange<T>();
            
            _stateMachine.ChangeState<T>();
            _gameStateMachineObserver.SendStateChanged();
        }

        public bool TryGetCurrentState(out IState state) =>
            _stateMachine.TryGetCurrentState(out state);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LogStateChange<T>() where T : IState
        {
            Type type = typeof(T);
            string stateName = type.Name.GetNiceName();
            string log = Log.HandleLog(tag: "Game State Machine", log: $"Enter ><cb>{stateName}</cb>");
            Debug.Log(log);
        }
    }
}