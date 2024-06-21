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

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            _lineRenderer.SetPosition(index: 0, _pointOne.position);
            _lineRenderer.SetPosition(index: 1, _pointTwo.position);
        }

        private void FixedUpdate() =>
            _balloonRigidbody.AddForce(Vector3.up * _gravity, ForceMode.Acceleration);

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Init()
        {
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
    }
}