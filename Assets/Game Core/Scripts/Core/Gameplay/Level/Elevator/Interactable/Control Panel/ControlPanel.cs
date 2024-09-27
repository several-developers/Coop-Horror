using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Elevator;
using GameCore.Gameplay.Entities.Level.Elevator;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class ControlPanel : SoundProducerMonoBehaviour<ElevatorEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            var elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
            SoundReproducer = new ElevatorSoundReproducer(soundProducer: this, elevatorConfig);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private List<ControlPanelButton> _panelButtons;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SetupPanelButtons();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPanelButtons()
        {
            foreach (ControlPanelButton panelButton in _panelButtons)
                panelButton.OnButtonClickedEvent += OnButtonClicked;
        }

        private void PlayButtonPushSound() => PlaySound(ElevatorEntity.SFXType.ButtonPush).Forget();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonClicked() => PlayButtonPushSound();
    }
}