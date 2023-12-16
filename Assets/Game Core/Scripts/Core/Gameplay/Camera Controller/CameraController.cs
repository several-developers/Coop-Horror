using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using GameCore.Gameplay.Observers.Taps;
using Sirenix.OdinInspector;
using Zenject;

namespace GameCore.Gameplay
{
    public class CameraController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITapsObserver tapsObserver) =>
            _tapsObserver = tapsObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Image _clickArea;

        [SerializeField, Required]
        private CinemachineFreeLook _freeLookCamera;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";
        
        private ITapsObserver _tapsObserver;
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect: _clickArea.rectTransform, 
                    screenPoint: eventData.position,
                    cam: eventData.enterEventCamera,
                    localPoint: out Vector2 posOut))
            {
                _freeLookCamera.m_XAxis.m_InputAxisName = MouseX;
                _freeLookCamera.m_YAxis.m_InputAxisName = MouseY;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
            _tapsObserver.SendTapDownEvent();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _tapsObserver.SendTapUpEvent();
            
            _freeLookCamera.m_XAxis.m_InputAxisName = string.Empty;
            _freeLookCamera.m_YAxis.m_InputAxisName = string.Empty;
            _freeLookCamera.m_XAxis.m_InputAxisValue = 0;
            _freeLookCamera.m_YAxis.m_InputAxisValue = 0;
        }
    }
}