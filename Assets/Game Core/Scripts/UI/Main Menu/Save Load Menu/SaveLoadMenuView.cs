using GameCore.UI.Global.MenuView;

namespace GameCore.UI.MainMenu.SaveLoadMenu
{
    public class SaveLoadMenuView : MenuView
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            DestroyOnHide();
        }

        private void Start()
        {
            Show();
        }
    }
}