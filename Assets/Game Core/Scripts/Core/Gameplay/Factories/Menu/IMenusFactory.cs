using System;
using Cysharp.Threading.Tasks;
using GameCore.UI.Global.MenuView;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories.Menu
{
    public interface IMenusFactory
    {
        UniTask<GameObject> Create(Type menuType);
        UniTask<GameObject> Create<TPayload>(Type menuType, TPayload param);
        UniTask<TMenu> Create<TMenu>() where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(DiContainer diContainer) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(Transform parent) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(Transform parent, DiContainer diContainer) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu, TPayload>(TPayload param) where TMenu : MenuView, IComplexMenuView<TPayload>;

        UniTask<TMenu> Create<TMenu, TPayload>(TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>;

        UniTask<TMenu> Create<TMenu, TPayload>(Transform parent, TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>;
    }
}