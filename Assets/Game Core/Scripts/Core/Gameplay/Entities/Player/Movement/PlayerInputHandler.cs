using System.Collections;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PlayerInputHandler : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        public float _speed = 5;
        public float _jumpHeight = 15;
        public PlayerMovement _playerMovement;

        public Transform _bodyRender;

        // FIELDS: --------------------------------------------------------------------------------

        private IEnumerator _sitCO;
        public bool _isSitting;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            Vector3 moveInput =
                Vector3.ClampMagnitude(transform.forward * vertical + transform.right * horizontal, maxLength: 1f) *
                _speed;

            _playerMovement.SetMoveInput(moveInput);

            if (!_playerMovement.IsGround)
                return;
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 inertiaVelocity = _playerMovement.InertiaVelocity;
                inertiaVelocity.y = 0f;
                inertiaVelocity.y += _jumpHeight;
                
                _playerMovement.SetInertiaVelocity(inertiaVelocity);
            }

            if (Input.GetKeyDown(KeyCode.C) && _sitCO == null)
            {
                _sitCO = SitDown();
                StartCoroutine(_sitCO);
            }
        }

        private IEnumerator SitDown()
        {
            if (_isSitting && Physics.Raycast(transform.position, Vector3.up,
                    _playerMovement.CharacterController.height * 1.5f))
            {
                _sitCO = null;
                yield break;
            }

            _isSitting = !_isSitting;

            float t = 0;
            float startSize = _playerMovement.CharacterController.height;
            float finalSize =
                _isSitting
                    ? _playerMovement.CharacterController.height / 2
                    : _playerMovement.CharacterController.height * 2;

            Vector3 startBodySize = _bodyRender.localScale;
            Vector3 finalBodySize = _isSitting
                ? _bodyRender.localScale - Vector3.up * _bodyRender.localScale.y / 2f
                : _bodyRender.localScale + Vector3.up * _bodyRender.localScale.y;


            _speed = _isSitting ? _speed / 2 : _speed * 2;
            _jumpHeight = _isSitting ? _jumpHeight * 3 : _jumpHeight / 3;

            while (t < 0.2f)
            {
                t += Time.deltaTime;
                _playerMovement.CharacterController.height = Mathf.Lerp(startSize, finalSize, t / 0.2f);
                _bodyRender.localScale = Vector3.Lerp(startBodySize, finalBodySize, t / 0.2f);
                yield return null;
            }

            _sitCO = null;
            yield break;
        }
    }
}