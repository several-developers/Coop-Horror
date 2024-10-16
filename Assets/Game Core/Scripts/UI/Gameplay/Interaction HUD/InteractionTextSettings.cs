﻿using System;
using GameCore.Enums.Gameplay;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.Interaction
{
    [Serializable]
    public class InteractionTextSettings
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private string _defaultDisabledInteractionText = "Can't interact";
        
        [SerializeField, Space(height: 5)]
        private TextSettings[] _interactionEnabledTextSettings;

        [SerializeField]
        private TextSettings[] _interactionDisabledTextSettings;

        // FIELDS: --------------------------------------------------------------------------------

        private TextMeshProUGUI _textTMP;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(TextMeshProUGUI textTMP) =>
            _textTMP = textTMP;

        public void UpdateText(InteractionType interactionType, bool canInteract)
        {
            string text = GetText(interactionType, canInteract);
            _textTMP.text = text;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private string GetText(InteractionType interactionType, bool canInteract)
        {
            TextSettings[] textSettings = canInteract
                ? _interactionEnabledTextSettings
                : _interactionDisabledTextSettings;

            foreach (TextSettings settings in textSettings)
            {
                bool isMatches = settings.InteractionType == interactionType;

                if (isMatches)
                    return settings.Text;
            }

            if (canInteract)
                return "Text Not Found.";
            
            return _defaultDisabledInteractionText;
        }

        [Serializable]
        public class TextSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private InteractionType _interactionType;

            [SerializeField]
            private string _text = "text";

            // PROPERTIES: ----------------------------------------------------------------------------

            public InteractionType InteractionType => _interactionType;
            public string Text => _text;
        }
    }
}