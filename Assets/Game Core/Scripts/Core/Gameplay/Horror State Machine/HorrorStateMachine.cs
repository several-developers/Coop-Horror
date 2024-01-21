using System;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class HorrorStateMachine : IHorrorStateMachine
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HorrorStateMachine() =>
            _stateMachine = new StateMachine();

        // FIELDS: --------------------------------------------------------------------------------

        private readonly StateMachine _stateMachine;

        private IState _currentState;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddState(IState state) =>
            _stateMachine.AddStateWithRemove(state);

        public void ChangeState<TState>() where TState : IState
        {
            //LogStateChange<TState>();
            
            _stateMachine.ChangeState<TState>();
            //_gameStateMachineObserver.SendStateChanged();
        }

        public void ChangeState<TState, TEnterParams>(TEnterParams enterParams) where TState : IState
        {
            //LogStateChange<TState>();
          
            _stateMachine.ChangeState<TState, TEnterParams>(enterParams);
            //_gameStateMachineObserver.SendStateChanged();
        }

        public bool TryGetCurrentState(out IState state) =>
            _stateMachine.TryGetCurrentState(out state);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LogStateChange<T>() where T : IState
        {
            Type type = typeof(T);
            string stateName = type.Name.GetNiceName();
            string log = Log.HandleLog(tag: "Horror State Machine", log: $"Enter ><cb>{stateName}</cb>");
            Debug.Log(log);
        }
    }
}