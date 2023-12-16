using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Global.Buttons
{
    public abstract class BaseButton : MonoBehaviour
    {
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        protected virtual void Awake() =>
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract void ClickLogic();
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonClicked() => ClickLogic();
    }
}