namespace GameCore.UI.Global.MenuView
{
    public interface IComplexMenuView<in T> : IMenuView
    {
        void Setup(T menuParams);
    }
}