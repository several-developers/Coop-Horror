using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class ControlPanel : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ControlPanelButton> _panelButtons;

        // FIELDS: --------------------------------------------------------------------------------

        private RpcCaller _rpcCaller;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SetupPanelButtons();

        private void Start() =>
            _rpcCaller = RpcCaller.Get();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator(Floor floor) =>
            _rpcCaller.StartElevator(floor);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPanelButtons()
        {
            foreach (ControlPanelButton panelButton in _panelButtons)
                panelButton.OnStartElevatorClickedEvent += OnStartElevatorClicked;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartElevatorClicked(Floor floor) => StartElevator(floor);
    }
}