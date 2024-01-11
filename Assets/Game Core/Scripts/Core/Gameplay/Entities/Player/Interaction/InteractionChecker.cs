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
            _screenCenterPoint = new Vector3(x: 0.5f, y: 0.5f);
            _hits = new RaycastHit[24];
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IPlayerInteractionObserver _playerInteractionObserver;
        private readonly Transform _playerTransform;
        private readonly Camera _camera;
        private readonly float _interactionMaxDistance;
        private readonly LayerMask _interactionLm;
        private readonly LayerMask _interactionObstaclesLM;
        private readonly Vector3 _screenCenterPoint;
        private readonly RaycastHit[] _hits;

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

            if (IsObstaclesFound(out int interactableItemIndex))
            {
                SendInteractionEndedEvent();
                return;
            }

            SendCanInteract(interactableItemIndex);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendInteractionEndedEvent()
        {
            if (!_isInteractionFound)
                return;

            _isInteractionFound = false;
            _playerInteractionObserver.SendInteractionEnded();
        }

        private void SendCanInteract(int interactableItemIndex)
        {
            if (_isInteractionFound)
                return;

            _isInteractionFound = true;
            var interactable = _hits[interactableItemIndex].transform.GetComponent<IInteractable>();

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
            bool isInteractableObjectFound =
                Physics.Raycast(ray, out RaycastHit hitInfo, _interactionMaxDistance, _interactionLm);

            if (!isInteractableObjectFound)
                return false;
            
            bool isInteractableComponentExists = hitInfo.transform.TryGetComponent(out IInteractable interactable);

            if (!isInteractableComponentExists)
                return false;
            
            if (!interactable.CanInteract())
                return false;

            return true;
        }

        private bool IsObstaclesFound(out int interactableItemIndex)
        {
            Ray ray = GetRay();
            int hitsAmount = Physics.RaycastNonAlloc(ray, _hits, _interactionMaxDistance, _interactionObstaclesLM);

            if (hitsAmount == 0)
            {
                interactableItemIndex = 0;
                return false;
            }

            int closestObjectIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < hitsAmount; i++)
            {
                RaycastHit hitInfo = _hits[i];
                float distance = Vector3.Distance(_playerTransform.position, hitInfo.point);
                //Debug.Log($"Hit: '{hitInfo.transform.name}',  Distance: '{distance:F2}'");
                
                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestObjectIndex = i;
            }

            interactableItemIndex = closestObjectIndex;
            RaycastHit closestObjectHitInfo = _hits[closestObjectIndex];
            int objectLayer = closestObjectHitInfo.transform.gameObject.layer;
            bool isObstacle = !_interactionLm.Contains(objectLayer);

            return isObstacle;
        }

        private Ray GetRay()
        {
            Vector3 screenCenterPoint = new(x: Screen.width * 0.5f, y: Screen.height * 0.5f);
            return _camera.ScreenPointToRay(screenCenterPoint);
        }
    }
}