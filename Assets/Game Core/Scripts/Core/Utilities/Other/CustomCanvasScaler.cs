using GameCore.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.Utilities
{
    public class CustomCanvasScaler : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField]
        private bool _useWidthOnPhone;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => UpdateCanvas();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateCanvas()
        {
            int phoneWidth = _useWidthOnPhone ? 0 : 1;
            int tabletWidth = _useWidthOnPhone ? 1 : 0;
            bool isPhone = DeviceTypeChecker.GetDeviceName() == DeviceName.Phone;
            float matchWidth = isPhone ? phoneWidth : tabletWidth;
            GetComponent<CanvasScaler>().matchWidthOrHeight = matchWidth;
        }
    }
}