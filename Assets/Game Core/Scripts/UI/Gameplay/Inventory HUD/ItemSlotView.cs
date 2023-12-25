using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Gameplay.Inventory
{
    public class ItemSlotView : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: Constants.Visualizer)]
        [BoxGroup(Constants.VisualizerIn, showLabel: false), SerializeField]
        private ItemSlotVisualizer _itemSlotVisualizer;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Select() =>
            _itemSlotVisualizer.Select();

        public void Deselect() =>
            _itemSlotVisualizer.Deselect();
    }
}