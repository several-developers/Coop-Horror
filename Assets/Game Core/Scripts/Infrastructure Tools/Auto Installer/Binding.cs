using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.InfrastructureTools.AutoInstaller
{
    [Serializable]
    public class Binding
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [ShowInInspector]
        [TypeSelectorSettings(FilterTypesFunction = nameof(TypeFilter), ShowCategories = true)]
        [ShowIf(nameof(ShowSelectedType))]
        [OnValueChanged(nameof(OnSelectedTypeChanged))]
        private Type _selectedType;

        [SerializeField]
        [InlineButton(action: nameof(ToggleSelectedType), SdfIconType.ArrowCounterclockwise, label: "")]
        [InlineButton(action: nameof(DebugValidateType), SdfIconType.Check2, label: "")]
        private string _savedType = "none";

        [SerializeField]
        private BindType _bindType = BindType.BindInterfacesTo;
        
        [SerializeField]
        private bool _nonLazy;

        // PROPERTIES: ----------------------------------------------------------------------------

        public BindType BindType => _bindType;
        public bool NonLazy => _nonLazy;

        private string Label => $"'Type: {GetTypeName()}',    'Binding Type: {GetBindingName()}'";
        private bool ShowSelectedType => _showSelectedType || _savedType.Equals("none");

        // FIELDS: --------------------------------------------------------------------------------

        private bool _showSelectedType;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Type GetSavedType()
        {
            var type = Type.GetType(_savedType);
            bool isValid = type != null;

            if (!isValid)
                LogTypeNotFoundError();

            return type;
        }

        public bool ValidateType()
        {
            var type = Type.GetType(_savedType);
            bool isValid = type != null;

            if (!isValid)
                LogTypeNotFoundError();

            return isValid;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DebugValidateType()
        {
            bool isValid = ValidateType();

            if (!isValid)
                return;

            Debug.Log(message: "Type is valid.");
        }

        private void ToggleSelectedType() =>
            _showSelectedType = !_showSelectedType;

        private void LogTypeNotFoundError() =>
            Debug.LogError(message: $"Тип '{_savedType}' не найден!");

        private string GetTypeName()
        {
            string result = _savedType.Substring(_savedType.LastIndexOf('.') + 1);
            Console.WriteLine(result); // Вывод: И еще одна точка.
            return result;
        }

        private string GetBindingName() =>
            _nonLazy ? $"{_bindType}, NonLazy" : $"{_bindType}";

        private bool TypeFilter(Type type)
        {
            return type
                .GetInterfaces()
                .Any(i => i.Namespace != null && !i.IsGenericType && i.Namespace.Contains("GameCore"));
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSelectedTypeChanged()
        {
            _savedType = _selectedType.ToString();
            _selectedType = null;
            _showSelectedType = false;
        }
    }
}