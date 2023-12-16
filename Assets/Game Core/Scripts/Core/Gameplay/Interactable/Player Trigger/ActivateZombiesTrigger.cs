using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GameCore.Gameplay.Interactable
{
    public class ActivateZombiesTrigger : PlayerTriggerBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Events)]
        [SerializeField]
        private UnityEvent _onTriggerEvent;
        
        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override void TriggerLogic() =>
            _onTriggerEvent.Invoke();
    }
}