using System;
using Cinemachine;
using DG.Tweening;
using GameCore.Configs.Gameplay.Train;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    public class PathMovement
    {
        public enum MovementType
        {
            Cycle = 0,
            SlowingDown = 1,
            SpeedingUp = 2
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PathMovement(TrainEntity trainEntity, TrainConfigMeta trainConfig)
        {
            _trainConfig = trainConfig;
            _transform = trainEntity.transform;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnDestinationReachedEvent;

        private readonly TrainConfigMeta _trainConfig;
        private readonly Transform _transform;

        private CinemachinePathBase _path;
        private Tweener _moveSpeedTN;
        private MovementType _movementType = MovementType.Cycle;
        private float _distance;
        private float _syncCurrentTime;
        private bool _isArrived;
        private bool _canMove = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Movement()
        {
            if (_path == null)
                return;

            if (_movementType != MovementType.Cycle)
            {
                UpdateTrainPosition();
                return;
            }

            float movementSpeed = _trainConfig.MovementSpeed2;
            _distance += movementSpeed * Time.deltaTime;

            float pathLength = GetPathLength();

            if (_distance >= pathLength)
            {
                if (!_isArrived)
                {
                    _isArrived = true;
                    OnDestinationReachedEvent?.Invoke();
                }
                else
                {
                    _distance = pathLength;
                }
            }

            Move(_distance, movementSpeed);
        }

        public void ToggleMovement(bool canMove) =>
            _canMove = canMove;

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

        public void SetMovementType(MovementType movementType) =>
            _movementType = movementType;

        public void SpeedUpTrain()
        {
            _moveSpeedTN.Kill();

            float duration = _trainConfig.SpeedUpDuration;
            float pathLength = GetPathLength();
            Ease ease = _trainConfig.SpeedUpEase;

            _moveSpeedTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    _distance = Mathf.Lerp(a: 0f, b: pathLength, t);
                })
                .SetEase(ease)
                .OnComplete(() =>
                {
                    _isArrived = true;
                    OnDestinationReachedEvent?.Invoke();
                });
        }

        public void SlowDownTrain()
        {
            _moveSpeedTN.Kill();

            float duration = _trainConfig.SlowDownDuration;
            float pathLength = GetPathLength();
            Ease ease = _trainConfig.SlowDownEase;

            _moveSpeedTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    _distance = Mathf.Lerp(a: 0f, b: pathLength, t);
                })
                .SetEase(ease)
                .OnComplete(() =>
                {
                    _isArrived = true;
                    OnDestinationReachedEvent?.Invoke();
                });
        }

        public void ResetDistance()
        {
            if (_path == null)
                return;

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

        private void UpdateTrainPosition()
        {
            Vector3 position = EvaluatePositionAtUnit(_distance);
            Quaternion rotation = EvaluateOrientationAtUnit(_distance);
            
            _transform.position = position;
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

        private float GetNormalizedPosition()
        {
            float pathLength = GetPathLength();
            float distance = _distance > pathLength ? pathLength : _distance;
            float normalizedPosition = distance / pathLength;
            return normalizedPosition;
        }

        private float GetPathLength() =>
            _path.PathLength;

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