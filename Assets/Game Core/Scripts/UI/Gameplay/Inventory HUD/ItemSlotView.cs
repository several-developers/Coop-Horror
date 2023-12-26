using GameCore.UI.Global.Animations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Gameplay.Inventory
{
    public class ItemSlotView : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(Constants.Visualizer)]
        [BoxGroup(Constants.VisualizerIn, showLabel: false), SerializeField]
        private ItemSlotVisualizer _itemSlotVisualizer;

        [TitleGroup(Constants.Animation)]
        [BoxGroup(Constants.AnimationIn, showLabel: false), SerializeField]
        private ScaleAnimation _scaleAnimation;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Select()
        {
            _itemSlotVisualizer.Select();
            _scaleAnimation.ScaleUp();
        }

        public void Deselect()
        {
            _itemSlotVisualizer.Deselect();
            _scaleAnimation.ScaleDown();
        }
    }
}