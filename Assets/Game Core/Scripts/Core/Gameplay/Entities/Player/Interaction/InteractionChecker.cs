using System;
using GameCore.Gameplay.Interactable;
using GameCore.Observers.Gameplay.PlayerInteraction;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Interaction
{
    public class InteractionChecker
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public InteractionChecker(IPlayerInteractionObserver playerInteractionObserver, Transform playerTransform,
            Camera camera, float interactionMaxDistance, LayerMask interactionLM, LayerMask interactionObstaclesLM)
        {
            _playerInteractionObserver = playerInteractionObserver;
            _playerTransform = playerTransform;
            _camera = camera;
            _interactionMaxDistance = interactionMaxDistance;
            _interactionLm = interactionLM;
            _interactionObstaclesLM = interactionObstaclesLM;
            _interactableHits = new RaycastHit[12];
            _obstaclesHits = new RaycastHit[24];
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IPlayerInteractionObserver _playerInteractionObserver;
        private readonly Transform _playerTransform;
        private readonly Camera _camera;
        private readonly float _interactionMaxDistance;
        private readonly LayerMask _interactionLm;
        private readonly LayerMask _interactionObstaclesLM;
        private readonly RaycastHit[] _interactableHits;
        private readonly RaycastHit[] _obstaclesHits;

        private IInteractable _lastInteractable;
        private int _lastInteractableItemIndex;
        private bool _isInteractionFound;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Check(bool drawDebugRay = false)
        {
            if (drawDebugRay)
                DrawDebugRay();

            if (!IsInteractableObjectFound())
            {
                SendInteractionEndedEvent();
                return;
            }

            if (IsObstaclesFound(out IInteractable interactable))
            {
                SendInteractionEndedEvent();
                return;
            }

            SendCanInteract(interactable);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendInteractionEndedEvent()
        {
            if (!_isInteractionFound)
                return;

            _isInteractionFound = false;
            _playerInteractionObserver.SendInteractionEnded();
        }

        private void SendCanInteract(IInteractable interactable)
        {
            if (_isInteractionFound && interactable == _lastInteractable)
                return;

            _lastInteractable = interactable;
            _isInteractionFound = true;

            _playerInteractionObserver.SendCanInteract(interactable);
        }

        private void DrawDebugRay()
        {
            Ray ray = GetRay();
            Debug.DrawRay(ray.origin, ray.direction * _interactionMaxDistance, Color.red);
        }

        private bool IsInteractableObjectFound()
        {
            Ray ray = GetRay();
            int hits = Physics.RaycastNonAlloc(ray, _interactableHits, _interactionMaxDistance, _interactionLm);

            if (hits == 0)
                return false;

            Vector3 playerPosition = _playerTransform.position;
            float closestDistance = float.MaxValue;
            int closestInteractableIndex = 0;
            bool isInteractableComponentExists;

            for (int i = 0; i < hits; i++)
            {
                Transform interactableTransform = _interactableHits[i].transform;
                isInteractableComponentExists = interactableTransform.TryGetComponent(out IInteractable interactable);

                if (!isInteractableComponentExists)
                {
                    Log.PrintError(log: $"Object <gb>{_interactableHits[i].transform.name}</gb> " +
                                        $"<rb>missing interactable component</rb>!");
                    continue;
                }

                Vector3 interactablePosition = interactableTransform.position;
                float distance = Vector3.Distance(playerPosition, interactablePosition);

                if (distance >= closestDistance)
                    continue;

                closestInteractableIndex = i;
                closestDistance = distance;
            }

            Transform closestInteractable = _interactableHits[closestInteractableIndex].transform;
            isInteractableComponentExists = closestInteractable.TryGetComponent(out IInteractable _);

            if (!isInteractableComponentExists)
                return false;

            return true;
        }

        private bool IsObstaclesFound(out IInteractable interactable)
        {
            Ray ray = GetRay();
            int hitsAmount =
                Physics.RaycastNonAlloc(ray, _obstaclesHits, _interactionMaxDistance, _interactionObstaclesLM);
            interactable = null;

            if (hitsAmount == 0)
                return false;

            int closestObjectIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < hitsAmount; i++)
            {
                RaycastHit hitInfo = _obstaclesHits[i];
                float distance = Vector3.Distance(_playerTransform.position, hitInfo.point);
                //Debug.Log($"Hit: '{hitInfo.transform.name}',  Distance: '{distance:F2}'");

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestObjectIndex = i;
            }

            RaycastHit closestObjectHitInfo = _obstaclesHits[closestObjectIndex];
            int objectLayer = closestObjectHitInfo.transform.gameObject.layer;
            bool isObstacle = !_interactionLm.Contains(objectLayer);

            if (!isObstacle)
                interactable = closestObjectHitInfo.transform.GetComponent<IInteractable>();

            return isObstacle;
        }

        private Ray GetRay()
        {
            Vector3 screenCenterPoint = new(x: Screen.width * 0.5f, y: Screen.height * 0.5f);
            return _camera.ScreenPointToRay(screenCenterPoint);
        }
    }
}