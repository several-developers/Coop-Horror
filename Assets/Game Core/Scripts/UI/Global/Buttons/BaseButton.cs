using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Global.Buttons
{
    public abstract class BaseButton : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        protected Button Button;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        protected virtual void Awake()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(OnButtonClicked);
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract void ClickLogic();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonClicked() => ClickLogic();
    }
}