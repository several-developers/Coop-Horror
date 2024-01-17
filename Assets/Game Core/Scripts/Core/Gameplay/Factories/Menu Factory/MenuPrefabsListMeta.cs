using GameCore.UI.Global.MenuView;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Factories
{
    public class MenuPrefabsListMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required, AssetsOnly]
        [ListDrawerSettings(AlwaysAddDefaultValue = true)]
        private MenuView[] _menuPrefabs;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public MenuView[] GetMenuPrefabs() => _menuPrefabs;
    }
}