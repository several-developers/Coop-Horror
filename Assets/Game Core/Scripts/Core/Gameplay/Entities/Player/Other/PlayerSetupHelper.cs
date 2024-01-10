using GameCore.Gameplay.Factories.ItemsPreview;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Player.Other
{
    public class PlayerSetupHelper : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IItemsPreviewFactory itemsPreviewFactory) =>
            _itemsPreviewFactory = itemsPreviewFactory;

        // FIELDS: --------------------------------------------------------------------------------

        private static PlayerSetupHelper _instance;
        
        private IItemsPreviewFactory _itemsPreviewFactory;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _instance = this;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IItemsPreviewFactory GetItemsPreviewFactory() => _itemsPreviewFactory;
        
        public static PlayerSetupHelper Get() => _instance;
    }
}