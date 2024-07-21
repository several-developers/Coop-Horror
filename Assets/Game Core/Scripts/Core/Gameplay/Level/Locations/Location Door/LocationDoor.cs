using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationDoor : NetcodeBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _animationDuration = 0.5f;

        [SerializeField, Min(0f)]
        private float _autoCloseDelay = 4f;

        [SerializeField, Min(0f)]
        private float _moveAmount = 2f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private SimpleButton _buttonOne;
        
        [SerializeField, Required]
        private SimpleButton _buttonTwo;

        [SerializeField, Required]
        private Transform _leftDoor;
        
        [SerializeField, Required]
        private Transform _rightDoor;

        // FIELDS: --------------------------------------------------------------------------------

        private Tweener _doorTN;
        private bool _isOpen;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            _buttonOne.OnTriggerEvent += OnButtonClicked;
            _buttonTwo.OnTriggerEvent += OnButtonClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void OpenDoorLogic()
        {
            _isOpen = true;
            
            PlayDoorAnimation(open: true);

            int delay = _autoCloseDelay.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            CloseDoorLogic();
        }

        private void CloseDoorLogic()
        {
            _isOpen = false;
            
            PlayDoorAnimation(open: false);
        }

        private void PlayDoorAnimation(bool open)
        {
            _doorTN.Complete();
            _doorTN.Kill();

            float leftDoorStartX = _leftDoor.localPosition.x;
            float rightDoorStartX = _rightDoor.localPosition.x;
            float leftDoorTargetX = leftDoorStartX + (open ? _moveAmount : -_moveAmount); 
            float rightDoorTargetX = rightDoorStartX + (open ? -_moveAmount : _moveAmount); 

            _doorTN = DOVirtual.Float(from: 0f, to: 1f, _animationDuration, onVirtualUpdate: (t) =>
            {
                float leftDoorX = Mathf.Lerp(a: leftDoorStartX, b: leftDoorTargetX, t);
                float rightDoorX = Mathf.Lerp(a: rightDoorStartX, b: rightDoorTargetX, t);

                Vector3 leftDoorPosition = _leftDoor.localPosition;
                Vector3 rightDoorPosition = _rightDoor.localPosition;

                leftDoorPosition.x = leftDoorX;
                rightDoorPosition.x = rightDoorX;

                _leftDoor.localPosition = leftDoorPosition;
                _rightDoor.localPosition = rightDoorPosition;
            });
        }
        
        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void OpenDoorServerRpc() => OpenDoorClientRpc();

        [ClientRpc]
        private void OpenDoorClientRpc() => OpenDoorLogic();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonClicked()
        {
            if (_isOpen)
                return;
            
            OpenDoorServerRpc();
        }
    }
}