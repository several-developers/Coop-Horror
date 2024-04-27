using GameCore.Gameplay.GameManagement;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.GoldCounter
{
    public class GoldCounterView : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _goldTMP;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _gameManagerDecorator.OnPlayersGoldChangedEvent += OnPlayersGoldChanged;

        private void OnDestroy() =>
            _gameManagerDecorator.OnPlayersGoldChangedEvent -= OnPlayersGoldChanged;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlayersGoldChanged(int playersGold) =>
            _goldTMP.text = $"Gold: {playersGold}";
    }
}