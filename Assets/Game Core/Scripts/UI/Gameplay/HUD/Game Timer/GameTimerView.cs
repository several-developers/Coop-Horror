using GameCore.Gameplay.Network;
using GameCore.UI.Global;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.HUD.GameTimer
{
    public class GameTimerView : UIElement
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _timeTMP;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            TheNetworkHorror networkHorror = TheNetworkHorror.Get();
            networkHorror.OnGameTimerUpdatedEvent += OnGameTimerUpdated;
            
            Show();
        }

        private void OnDestroy()
        {
            TheNetworkHorror networkHorror = TheNetworkHorror.Get();
            networkHorror.OnGameTimerUpdatedEvent -= OnGameTimerUpdated;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateTime(float time)
        {
            time.ConvertToMinutes(out string text);
            _timeTMP.text = text;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnGameTimerUpdated(float time) => UpdateTime(time);
    }
}