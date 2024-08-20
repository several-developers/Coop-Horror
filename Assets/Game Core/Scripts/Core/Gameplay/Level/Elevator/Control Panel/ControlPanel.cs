using System.Collections.Generic;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ControlPanel : SoundProducerMonoBehaviour<ElevatorBase.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IElevatorsManagerDecorator elevatorsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider
            )
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _elevatorConfig = gameplayConfigsProvider.GetElevatorConfig();
            _soundReproducer = new ElevatorSoundReproducer(soundProducer: this, _elevatorConfig);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ControlPanelButton> _panelButtons;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private ElevatorConfigMeta _elevatorConfig;
        private ElevatorSoundReproducer _soundReproducer;

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

        private void PlayButtonPushSound() =>
            _soundReproducer.PlaySound(ElevatorBase.SFXType.ButtonPush);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartElevatorClicked(Floor floor)
        {
            StartElevator(floor);
            PlayButtonPushSound();
        }
    }
}