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
        [BoxGroup(Constants.Animation + "/Selection Scale", showLabel: false), SerializeField]
        private ScaleAnimation _scaleAnimation;

        [BoxGroup(Constants.Animation + "/Interaction Scale", showLabel: false), SerializeField]
        private PunchScaleAnimation _punchScaleAnimation;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _scaleAnimation.SetOwner(gameObject);
            _punchScaleAnimation.SetOwner(gameObject);
        }

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

        public void SetIcon(Sprite sprite)
        {
            _punchScaleAnimation.PlayAnimation();
            _itemSlotVisualizer.SetIcon(sprite);
        }

        public void RemoveIcon()
        {
            _punchScaleAnimation.PlayAnimation();
            _itemSlotVisualizer.RemoveIcon();
        }
    }
}