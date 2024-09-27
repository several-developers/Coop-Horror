using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.InfrastructureTools.AutoInstaller
{
    // ---------------------------------------------------
    // -
    // -    Позволяет биндить сущности из редактора.
    // -
    // -    Примечание:
    // -    !! Не подходит для комплексных биндов.
    // -    !! Не подходит если нужна ссылка на Game Object.
    // -    !! Неизвестно насколько этот способ ресурсозатратный.
    // -    !! Неизвестно о возможных ошибках.
    // -
    // ---------------------------------------------------

    [CreateAssetMenu(fileName = "Auto Installer")]
    public class AutoInstaller : ScriptableObjectInstaller
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AutoInstaller()
        {
            _services = new List<Binding>();
            _other = new List<Binding>();
            _providers = new List<Binding>();
            _factories = new List<Binding>();
            _observers = new List<Binding>();
            _states = new List<Binding>();
            _groups = new List<GroupBinding>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(GroupTitle)]
        [TabGroup(GroupIn, ServicesTab, SdfIconType.GearWideConnected, TextColor = "green")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _services;

        [TabGroup(GroupIn, OtherTab, SdfIconType.PatchQuestion, TextColor = "yellow")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _other;

        [TabGroup(GroupIn, ProvidersTab, SdfIconType.ShareFill, TextColor = "orange")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _providers;

        [TabGroup(GroupIn, FactoriesTab, SdfIconType.BoxSeam, TextColor = "red")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _factories;

        [TabGroup(GroupIn, ObserversTab, SdfIconType.EyeFill, TextColor = "blue")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _observers;

        [TabGroup(GroupIn, StatesTab, SdfIconType.LayersHalf, TextColor = "purple")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _states;

        [TabGroup(GroupIn, GroupsTab, SdfIconType.Stack, TextColor = "lightgray")]
        [SerializeField, Searchable]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<GroupBinding> _groups;

        // FIELDS: --------------------------------------------------------------------------------

        private const string GroupTitle = "Group";
        private const string GroupIn = GroupTitle + "/In";

        private const string ServicesTab = "Services";
        private const string OtherTab = "Other";
        private const string ProvidersTab = "Providers";
        private const string FactoriesTab = "Factories";
        private const string ObserversTab = "Observers";
        private const string StatesTab = "States";
        private const string GroupsTab = "Groups";

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnEnable()
        {
            ValidateBindings(_services);
            ValidateBindings(_other);
            ValidateBindings(_providers);
            ValidateBindings(_factories);
            ValidateBindings(_observers);
            ValidateBindings(_states);
            ValidateBindings(_groups);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void InstallBindings()
        {
            HandleBinding(_services);
            HandleBinding(_other);
            HandleBinding(_providers);
            HandleBinding(_factories);
            HandleBinding(_observers);
            HandleBinding(_states);
            HandleBinding(_groups);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleBinding(List<GroupBinding> groupBindings)
        {
            foreach (GroupBinding groupBinding in groupBindings)
            {
                IEnumerable<Binding> bindings = groupBinding.GetBindings();
                HandleBinding(bindings);
            }
        }

        private void HandleBinding(IEnumerable<Binding> bindings)
        {
            foreach (Binding binding in bindings)
            {
                Type type = binding.GetSavedType();
                BindType bindType = binding.BindType;
                bool nonLazy = binding.NonLazy;

                HandleBinding(type, bindType, nonLazy);
            }
        }

        private void HandleBinding(Type type, BindType bindType, bool nonLazy)
        {
            switch (bindType)
            {
                case BindType.Bind:
                    Bind();
                    break;

                case BindType.BindInterfacesTo:
                    BindInterfacesTo();
                    break;

                case BindType.BindInterfacesAndSelfTo:
                    BindInterfacesAndSelfTo();
                    break;
            }

            // LOCAL METHODS: -----------------------------

            void Bind()
            {
                if (nonLazy)
                    Container.Bind(type).AsSingle().NonLazy();
                else
                    Container.Bind(type).AsSingle();
            }

            void BindInterfacesTo()
            {
                if (nonLazy)
                    Container.BindInterfacesTo(type).AsSingle().NonLazy();
                else
                    Container.BindInterfacesTo(type).AsSingle();
            }

            void BindInterfacesAndSelfTo()
            {
                if (nonLazy)
                    Container.BindInterfacesAndSelfTo(type).AsSingle().NonLazy();
                else
                    Container.BindInterfacesAndSelfTo(type).AsSingle();
            }
        }

        private static void ValidateBindings(List<GroupBinding> groupsBinding)
        {
            foreach (GroupBinding groupBinding in groupsBinding)
            {
                IEnumerable<Binding> bindings = groupBinding.GetBindings();
                ValidateBindings(bindings);
            }
        }

        private static void ValidateBindings(IEnumerable<Binding> bindings)
        {
            foreach (Binding binding in bindings)
            {
                if (binding == null)
                {
                    Debug.LogError(message: "Binding is null!");
                    break;
                }

                bool isTypeValid = binding.ValidateType();

                if (!isTypeValid)
                    continue;
            }
        }
    }
}