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

            float leftDoorStartZ = _leftDoor.localPosition.z;
            float rightDoorStartZ = _rightDoor.localPosition.z;
            float leftDoorTargetZ = leftDoorStartZ + (open ? _moveAmount : -_moveAmount); 
            float rightDoorTargetZ = rightDoorStartZ + (open ? -_moveAmount : _moveAmount); 

            _doorTN = DOVirtual.Float(from: 0f, to: 1f, _animationDuration, onVirtualUpdate: (t) =>
            {
                float leftDoorZ = Mathf.Lerp(a: leftDoorStartZ, b: leftDoorTargetZ, t);
                float rightDoorZ = Mathf.Lerp(a: rightDoorStartZ, b: rightDoorTargetZ, t);

                Vector3 leftDoorPosition = _leftDoor.localPosition;
                Vector3 rightDoorPosition = _rightDoor.localPosition;

                leftDoorPosition.z = leftDoorZ;
                rightDoorPosition.z = rightDoorZ;

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