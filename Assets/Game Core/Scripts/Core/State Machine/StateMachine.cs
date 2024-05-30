using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class StateMachine
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action<IState> OnStateChangedEvent = delegate { }; 

        private readonly Dictionary<Type, IState> _states = new();
        
        private IState _currentState;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void FixedTick()
        {
            if (_currentState is IFixedTickableState state)
                state.FixedTick();
        }

        public void LateTick()
        {
            if (_currentState is ILateTickableState state)
                state.LateTick();
        }

        public void Tick()
        {
            if (_currentState is ITickableState state)
                state.Tick();
        }

        public void AddState<T>(T state) where T : IState
        {
            Type type = state.GetType();

            if (IsStateExists(type))
                return;

            _states.Add(type, state);
        }
        
        public void AddStateWithRemove<T>(T state) where T : IState
        {
            Type type = state.GetType();

            if (IsStateExists(type))
                RemoveState(type);

            _states.Add(type, state);
        }

        public void ChangeState<TState>() where TState : IState
        {
            Type type = typeof(TState);

            if (!IsStateExists(type))
                return;

            switch (_currentState)
            {
                case IExitState exitState:
                    exitState.Exit();
                    break;
                case IExitStateAsync exitStateAsync:
                    exitStateAsync.Exit();
                    break;
            }

            _currentState = _states[type];
            OnStateChangedEvent.Invoke(_currentState);

            switch (_currentState)
            {
                case IEnterState enterState:
                    enterState.Enter();
                    break;
                case IEnterStateAsync enterStateAsync:
                    enterStateAsync.Enter();
                    break;
            }
        }

        public void ChangeState<TState, TEnterParams>(TEnterParams enterParams) where TState : IState
        {
            Type type = typeof(TState);

            if (!IsStateExists(type))
                return;

            switch (_currentState)
            {
                case IExitState exitState:
                    exitState.Exit();
                    break;
                case IExitStateAsync exitStateAsync:
                    exitStateAsync.Exit();
                    break;
            }

            _currentState = _states[type];
            OnStateChangedEvent.Invoke(_currentState);

            switch (_currentState)
            {
                case IEnterState enterState:
                    enterState.Enter();
                    break;
                
                case IEnterState<TEnterParams> enterState:
                    enterState.Enter(enterParams);
                    break;

                case IEnterStateAsync enterStateAsync:
                    enterStateAsync.Enter();
                    break;
            }
        }
        
        public bool TryGetCurrentState(out IState state)
        {
            state = _currentState;
            return _currentState != null;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void RemoveState(Type type) =>
            _states.Remove(type);

        private bool IsStateExists(Type type)
        {
            bool isStateExists = _states.ContainsKey(type);

            //if (!isStateExists)
                //LogStateDontExistsError(type);
            
            return isStateExists;
        }

        private static void LogStateDontExistsError(Type type)
        {
            string log = Log.HandleLog($"State of type <gb>({type})</gb> <rb>don't exists</rb>!");
            Debug.LogError(log);
        }
    }
}