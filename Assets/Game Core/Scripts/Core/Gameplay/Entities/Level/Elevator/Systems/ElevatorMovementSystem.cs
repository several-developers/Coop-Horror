using System;
using System.Collections;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    public class ElevatorMovementSystem
    {
        public enum MovementState
        {
            Idle = 0,
            MoveUpToTeleportPoint = 1,
            MoveDownToTeleportPoint = 2,
            MoveToStartPoint = 3
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorMovementSystem(ElevatorEntity elevatorEntity, ILevelProvider levelProvider)
        {
            _elevatorEntity = elevatorEntity;
            _elevatorConfig = elevatorEntity.GetElevatorConfig();
            _references = elevatorEntity.GetReferences();
            _levelProvider = levelProvider;
            _transform = elevatorEntity.transform;
            _movementLogicRoutine = new CoroutineHelper(elevatorEntity);
            _lastTeleportPointOrigin = _transform.position;

            _movementLogicRoutine.GetRoutineEvent += MovementLogicCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnElevatorStoppedEvent = delegate { };

        private readonly ElevatorEntity _elevatorEntity;
        private readonly ElevatorConfigMeta _elevatorConfig;
        private readonly ElevatorReferences _references;
        private readonly ILevelProvider _levelProvider;
        private readonly Transform _transform;
        private readonly CoroutineHelper _movementLogicRoutine;

        private Vector3 _lastTeleportPointOrigin;
        private MovementState _movementState;
        private bool _reachedTargetFloor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartMovement()
        {
            CheckMovementState();

            if (_movementState == MovementState.Idle)
                return;

            _movementLogicRoutine.Start();
        }

        public void StopMovement() =>
            _movementLogicRoutine.Stop();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckMovementState()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();

            if (currentFloor == targetFloor)
            {
                _movementState = MovementState.Idle;
                return;
            }

            int targetFloorIndex = (int)targetFloor;
            int currentFloorIndex = (int)currentFloor;

            _movementState = targetFloorIndex > currentFloorIndex
                ? MovementState.MoveDownToTeleportPoint
                : MovementState.MoveUpToTeleportPoint;
        }

        private void CheckIfReachedTargetFloor()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();
            _reachedTargetFloor = currentFloor == targetFloor;
        }

        private void ChangeFloor()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();

            if (currentFloor == targetFloor)
                return;

            int targetFloorIndex = (int)targetFloor;
            int currentFloorIndex = (int)currentFloor;

            int newFloorIndex = targetFloorIndex > currentFloorIndex
                ? currentFloorIndex + 1
                : currentFloorIndex - 1;

            var newFloor = (Floor)newFloorIndex;
            _elevatorEntity.SetCurrentFloor(newFloor);
        }

        private void TeleportToSection()
        {
            Floor currentFloor = GetCurrentFloor();

            bool isElevatorMovePointFound =
                _levelProvider.TryGetElevatorMovePoint(currentFloor, out ElevatorMovePoint elevatorMovePoint);

            if (!isElevatorMovePointFound)
                return;

            _lastTeleportPointOrigin = elevatorMovePoint.transform.position;

            Vector3 teleportPosition = _lastTeleportPointOrigin;
            float movementOffsetY = _elevatorConfig.MovementOffsetY;

            switch (_movementState)
            {
                case MovementState.MoveUpToTeleportPoint:
                    teleportPosition.y -= movementOffsetY;
                    break;

                case MovementState.MoveDownToTeleportPoint:
                    teleportPosition.y += movementOffsetY;
                    break;
            }

            ClientNetworkTransform networkTransform = _references.NetworkTransform;
            networkTransform.Teleport(teleportPosition, Quaternion.identity, newScale: Vector3.one);
        }

        private IEnumerator MovementLogicCO()
        {
            yield return MovementStartDelayCO();
            yield return MoveToTeleportPointCO();
            yield return ChangeFloorCO();
            
            TeleportToSection();
            
            _movementState = MovementState.MoveToStartPoint;
            
            yield return MoveToTeleportPointCO();
            
            float doorOpenDelay = _elevatorConfig.DoorOpenDelay;
            yield return new WaitForSeconds(doorOpenDelay);
            
            OnElevatorStoppedEvent?.Invoke();

            // LOCAL METHODS: -----------------------------

            IEnumerator MovementStartDelayCO()
            {
                float movementDelay = _elevatorConfig.MovementDelay;
                yield return new WaitForSeconds(movementDelay);
            }

            IEnumerator MoveToTeleportPointCO()
            {
                float startPositionY = _transform.position.y;
                float endPositionY = GetEndPositionY();
                float movementDuration = GetMovementDuration();
                float elapsedTime = 0f;

                while (elapsedTime < movementDuration)
                {
                    elapsedTime += Time.deltaTime;

                    float time = elapsedTime / movementDuration;
                    float interpolatedValue = GetInterpolatedValue(startPositionY, endPositionY, time);

                    _transform.position = 
                        new Vector3(x: _lastTeleportPointOrigin.x, y: interpolatedValue, z: _lastTeleportPointOrigin.z);

                    yield return null;
                }

                _transform.position =
                    new Vector3(x: _lastTeleportPointOrigin.x, y: endPositionY, z: _lastTeleportPointOrigin.z);
            }

            IEnumerator ChangeFloorCO()
            {
                float movementDuration = GetMovementDuration();
                
                do
                {
                    ChangeFloor();
                    CheckIfReachedTargetFloor();

                    if (_reachedTargetFloor)
                        break;

                    yield return new WaitForSeconds(movementDuration);
                } while (!_reachedTargetFloor);
            }
        }

        private AnimationCurve GetAnimationCurve()
        {
            return _movementState switch
            {
                MovementState.MoveToStartPoint => _elevatorConfig.SlowDownCurve,
                _ => _elevatorConfig.SpeedUpCurve
            };
        }

        private Floor GetCurrentFloor() =>
            _elevatorEntity.GetCurrentFloor();

        private Floor GetTargetFloor() =>
            _elevatorEntity.GetTargetFloor();

        private float GetMovementDuration() =>
            _elevatorConfig.MovementDurationPerFloor;

        private float GetInterpolatedValue(float startPositionY, float endPositionY, float time)
        {
            AnimationCurve animationCurve = GetAnimationCurve();
            return Mathf.Lerp(a: startPositionY, b: endPositionY, t: animationCurve.Evaluate(time));
        }

        private float GetEndPositionY()
        {
            float movementOffsetY = _elevatorConfig.MovementOffsetY;

            switch (_movementState)
            {
                case MovementState.Idle:
                case MovementState.MoveToStartPoint:
                default:
                    return _lastTeleportPointOrigin.y;

                case MovementState.MoveUpToTeleportPoint:
                    return _lastTeleportPointOrigin.y + movementOffsetY;

                case MovementState.MoveDownToTeleportPoint:
                    return _lastTeleportPointOrigin.y - movementOffsetY;
            }
        }
    }
}