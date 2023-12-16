namespace GameCore.UI.Global.MenuView
{
    public interface IMenuView
    {
        void Show();
        void Hide(bool ignoreDestroy = false);
    }
}