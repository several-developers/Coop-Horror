using System.Collections;
using DG.Tweening;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Triggers;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
#warning MAYBE MOVE OUT LOGIC TO SERVER AND SYNC ONLY NETWORK VARIABLE?
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
        private LocationDoorTrigger _firstDoorTrigger;

        [SerializeField, Required]
        private LocationDoorTrigger _secondDoorTrigger;

        [SerializeField, Required]
        private Transform _leftDoor;

        [SerializeField, Required]
        private Transform _rightDoor;

        // FIELDS: --------------------------------------------------------------------------------

        private Coroutine _doorAutoCloseTimerCO;
        private Tweener _doorTN;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _firstDoorTrigger.OnTriggeredEvent += OpenDoorServerRpc;
            _secondDoorTrigger.OnTriggeredEvent += OpenDoorServerRpc;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _firstDoorTrigger.OnTriggeredEvent -= OpenDoorServerRpc;
            _secondDoorTrigger.OnTriggeredEvent -= OpenDoorServerRpc;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OpenDoorLogic()
        {
            if (!_isOpen)
                PlayDoorAnimation(open: true);

            _isOpen = true;
            
            StopDoorAutoCloseTimer();
            StartDoorAutoCloseTimer();
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

        private void StartDoorAutoCloseTimer()
        {
            IEnumerator routine = DoorAutoCloseTimerCO();
            _doorAutoCloseTimerCO = StartCoroutine(routine);
        }

        private void StopDoorAutoCloseTimer()
        {
            if (_doorAutoCloseTimerCO == null)
                return;
            
            StopCoroutine(_doorAutoCloseTimerCO);
        }

        private IEnumerator DoorAutoCloseTimerCO()
        {
            yield return new WaitForSeconds(_autoCloseDelay);

            CloseDoorLogic();
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void OpenDoorServerRpc() => OpenDoorClientRpc();

        [ClientRpc]
        private void OpenDoorClientRpc() => OpenDoorLogic();
    }
}