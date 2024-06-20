using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ControlPanel : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ControlPanelButton> _panelButtons;

        // FIELDS: --------------------------------------------------------------------------------

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SetupPanelButtons();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartElevator(Floor floor) =>
            _elevatorsManagerDecorator.StartElevator(floor);

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