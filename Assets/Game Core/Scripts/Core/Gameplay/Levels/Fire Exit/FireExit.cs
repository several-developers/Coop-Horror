using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Interactable;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class FireExit : MonoBehaviour, IInteractable
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;

        [SerializeField]
        private bool _isInsideDungeon;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _teleportPoint;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;
        
        private bool _canInteract = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent?.Invoke();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.FireExitDoor;

        public bool CanInteract() => _canInteract;
    }
}