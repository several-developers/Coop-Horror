using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.MainMenu.SaveSelectionMenu
{
    public class SaveSelectionMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private SaveCellsController _saveCellsController;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            DestroyOnHide();
        }

        private void Start() => Show();
    }
}