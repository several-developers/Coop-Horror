using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ControlPanel : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IRpcHandlerDecorator rpcHandlerDecorator) =>
            _rpcHandlerDecorator = rpcHandlerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ControlPanelButton> _panelButtons;

        // FIELDS: --------------------------------------------------------------------------------

        private IRpcHandlerDecorator _rpcHandlerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SetupPanelButtons();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator(Floor floor) =>
            _rpcHandlerDecorator.StartElevator(floor);

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