using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class Balloon : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private float _gravity;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Rigidbody _anchorRigidbody;
        
        [SerializeField, Required]
        private Rigidbody _balloonRigidbody;

        [SerializeField, Required]
        private LineRenderer _lineRenderer;

        [SerializeField, Required]
        private Transform _pointOne;
        
        [SerializeField, Required]
        private Transform _pointTwo;
        
        [SerializeField, Required]
        private Transform _balloonAnchorContainer;

        // FIELDS: --------------------------------------------------------------------------------

        private const float MaxY = 1000f;
        
        private Transform _transform;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() =>
            _transform = transform;

        private void Update()
        {
            Vector3 position = _transform.position;
            bool isHeightValid = position.y < MaxY;

            if (!isHeightValid)
            {
                position.y = MaxY;
                _transform.position = position;
            }
            
            if (!_isInitialized)
                return;
            
            _lineRenderer.SetPosition(index: 0, _pointOne.position);
            _lineRenderer.SetPosition(index: 1, _pointTwo.position);
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
                return;
            
            _balloonRigidbody.AddForce(Vector3.up * _gravity, ForceMode.Acceleration);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Init()
        {
            _isInitialized = true;

            _pointOne.SetParent(_balloonAnchorContainer);
            _pointOne.localPosition = Vector3.zero;
            _pointOne.localRotation = Quaternion.Euler(Vector3.zero);

            transform.SetParent(p: null);
        }

        public void Release()
        {
            _anchorRigidbody.isKinematic = false;
            Destroy(gameObject, t: 500f);
        }

        public void Reset() =>
            _isInitialized = false;
    }
}