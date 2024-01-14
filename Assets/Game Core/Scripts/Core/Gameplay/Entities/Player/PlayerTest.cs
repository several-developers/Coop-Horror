using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerTest : MonoBehaviour
    {
        public float _groundDistance;
        public float _sphereCastRadius;
        public float _range;
        public float _jumpForce;
        public float _moveForce;

        public Vector3 _groundVelocity;
        
        public LayerMask _layerMask;

        public Transform _groundReference;

        // FIELDS: --------------------------------------------------------------------------------

        private Rigidbody _rigidbody;
        private Vector3 _desiredMovement;
        private bool _isGrounded;
        private bool _jump;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();

        private void Update()
        {
            float xAxis = Input.GetAxis("Horizontal");
            float yAxis = Input.GetAxis("Vertical");
            Vector3 movement = new(-xAxis, 0, -yAxis);

            _desiredMovement = movement;
            _isGrounded = Physics.CheckSphere(_groundReference.position, _groundDistance, _layerMask,
                QueryTriggerInteraction.Ignore);

            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
                _jump = true;

            RaycastHit hit;

            if (Physics.SphereCast(transform.position, _sphereCastRadius, Vector3.down * _range, out hit,
                    _range, _layerMask))
            {
                if (hit.transform.GetComponent<Rigidbody>() != null)
                {
                    _groundVelocity = hit.transform.GetComponent<Rigidbody>().velocity;
                }
                else
                {
                    _groundVelocity = Vector3.zero;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_jump)
            {
                _jump = false;
                _rigidbody.AddForce(Vector3.up * _jumpForce);
            }

            float x = (_desiredMovement.x * _moveForce) + _groundVelocity.x;
            float y = _rigidbody.velocity.y;
            float z = (_desiredMovement.z * _moveForce) + _groundVelocity.z;
            _rigidbody.velocity = new Vector3(x, y, z);
        }
    }
}