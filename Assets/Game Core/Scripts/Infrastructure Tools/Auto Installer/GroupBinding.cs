using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.InfrastructureTools.AutoInstaller
{
    [Serializable]
    public class GroupBinding
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GroupBinding() =>
            _bindings = new List<Binding>();

        // MEMBERS: -------------------------------------------------------------------------------

        [BoxGroup("Group", showLabel: false), SerializeField]
        private string _groupName = "none";
        
        [SerializeField, Searchable, Space(height: 5)]
        [ListDrawerSettings(ListElementLabelName = "Label", DefaultExpandedState = true, ShowFoldout = false)]
        private List<Binding> _bindings;

        // PROPERTIES: ----------------------------------------------------------------------------

        private string Label => $"'Group: {_groupName}',    'Bindings: {_bindings.Count}'";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<Binding> GetBindings() => _bindings;
    }
}