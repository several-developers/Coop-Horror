using System;
using Cinemachine;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    public class PathMovement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PathMovement(MobileHeadquartersEntity mobileHeadquartersEntity)
        {
            MobileHeadquartersReferences references = mobileHeadquartersEntity.References;
            
            _transform = mobileHeadquartersEntity.transform;
            _animator = references.Animator;
            _moveSpeedController = references.MoveSpeedController;

            _moveSpeedController.OnDistanceChangedEvent += OnDistanceChanged;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnDestinationReachedEvent;

        private readonly Transform _transform;
        private readonly Animator _animator;
        private readonly MoveSpeedController _moveSpeedController;

        private CinemachinePathBase _path;
        private float _animationBlend;
        private float _distance;
        private float _syncCurrentTime;
        private bool _isArrived;
        private bool _canMove = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            if (_path == null)
                return;

            float targetSpeed = _canMove ? _moveSpeedController.GetCurrentSpeed() : 0f;
            _distance += targetSpeed * Time.deltaTime;

            if (_distance > _path.PathLength)
            {
                if (!_isArrived)
                {
                    _isArrived = true;
                    OnDestinationReachedEvent?.Invoke();
                }
                else
                {
                    _distance = _path.PathLength;
                }
            }

            Move(_distance, targetSpeed);
        }

        public void ToggleMovement(bool canMove) =>
            _canMove = canMove;

        public void ToggleMoveAnimation(bool canMove) =>
            _animator.SetBool(id: AnimatorHashes.CanMove, value: canMove);

        public void ChangePath(CinemachinePath path, float startDistancePercent = 0f, bool stayAtSamePosition = false)
        {
            _path = path;

            if (stayAtSamePosition)
            {
                float newDistance = GetDistanceAtPosition(_transform.position);
                _distance = newDistance;
            }
            else
            {
                startDistancePercent = Mathf.Clamp01(startDistancePercent);

                if (startDistancePercent > 0.0f)
                    _distance = _path.PathLength * startDistancePercent;
                else
                    _distance = 0f;
            }

            Vector3 position = EvaluatePositionAtUnit(_distance);
            Quaternion rotation = EvaluateOrientationAtUnit(_distance);

            _transform.position = position;
            _transform.rotation = rotation;
        }

        public void ToggleArrived(bool isArrived) =>
            _isArrived = isArrived;

        public void ResetDistance()
        {
            float difference = _distance - _path.PathLength;
            _distance = difference;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Move(float distance, float targetSpeed)
        {
            Vector3 position = EvaluatePositionAtUnit(distance);
            Quaternion rotation = EvaluateOrientationAtUnit(distance);
            Vector3 movePosition = Vector3.MoveTowards(_transform.position, position, targetSpeed * Time.deltaTime);

            Debug.DrawLine(position, new Vector3(position.x, position.y + 2f, position.z), Color.red);
            Debug.DrawLine(movePosition, new Vector3(movePosition.x, movePosition.y + 2f, movePosition.z), Color.blue);

            _transform.position = movePosition;
            _transform.rotation = rotation;
        }

        private Vector3 EvaluatePositionAtUnit(float distance) =>
            _path.EvaluatePositionAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

        private Quaternion EvaluateOrientationAtUnit(float distance) =>
            _path.EvaluateOrientationAtUnit(distance, CinemachinePathBase.PositionUnits.Distance);

        private float GetDistanceAtPosition(Vector3 worldPosition)
        {
            float distanceAtPathUnits =
                _path.FindClosestPoint(worldPosition, startSegment: 0, searchRadius: -1, stepsPerSegment: 50);

            float pathDistance =
                _path.FromPathNativeUnits(distanceAtPathUnits, CinemachinePathBase.PositionUnits.Distance);
            
            return pathDistance;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDistanceChanged(float distance)
        {
            if (_path == null)
                return;

            float newDistance = _path.PathLength * distance;
            _distance = newDistance;
            
            Vector3 position = EvaluatePositionAtUnit(_distance);
            Quaternion rotation = EvaluateOrientationAtUnit(_distance);

            _transform.position = position;
            _transform.rotation = rotation;
        }
    }
}