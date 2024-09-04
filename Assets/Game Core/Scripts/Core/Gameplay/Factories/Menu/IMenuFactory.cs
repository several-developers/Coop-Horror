using System;
using Cysharp.Threading.Tasks;
using GameCore.UI.Global.MenuView;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Factories.Menu
{
    public interface IMenuFactory
    {
        UniTask<GameObject> Create(Type menuType);
        UniTask<GameObject> Create<TPayload>(Type menuType, TPayload param);
        UniTask<TMenu> Create<TMenu>() where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(DiContainer diContainer) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(Transform container) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu>(Transform container, DiContainer diContainer) where TMenu : MenuView;
        UniTask<TMenu> Create<TMenu, TPayload>(TPayload param) where TMenu : MenuView, IComplexMenuView<TPayload>;

        UniTask<TMenu> Create<TMenu, TPayload>(TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>;

        UniTask<TMenu> Create<TMenu, TPayload>(Transform container, TPayload param, DiContainer diContainer)
            where TMenu : MenuView, IComplexMenuView<TPayload>;
    }
}