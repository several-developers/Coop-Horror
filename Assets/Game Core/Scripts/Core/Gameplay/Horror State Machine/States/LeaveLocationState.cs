using System;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Locations;
using GameCore.Gameplay.Network;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationState(IHorrorStateMachine horrorStateMachine, ILocationsLoader locationsLoader)
        {
            _horrorStateMachine = horrorStateMachine;
            _locationsLoader = locationsLoader;
            
            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILocationsLoader _locationsLoader;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            RpcCaller rpcCaller = RpcCaller.Get();

            UnloadLastLocation();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UnloadLastLocation() =>
            _locationsLoader.UnloadLastLocationNetwork();
        
        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}