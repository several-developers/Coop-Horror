using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    public class TargetsSearcher
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TargetsSearcher(Camera camera)
        {
            _camera = camera;
            _screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            _hits = new RaycastHit[10];
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Camera _camera;
        private readonly LayerMask _aimObstaclesLayerMask;
        private readonly LayerMask _environmentLayerMask;
        private readonly Vector3 _screenCenterPoint;
        private readonly RaycastHit[] _hits;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool FindTarget(bool drawRay = false)
        {
            //Vector3 eyesPosition = _eyePosition.position;
            //Vector3 direction = (_aimTarget.position - eyesPosition).normalized;
            //bool isTargetFound = Physics.Raycast(eyesPosition, direction, maxShootDistance, _targetLayerMask);

            Ray ray = _camera.ScreenPointToRay(_screenCenterPoint);
            float maxShootDistance = 10;
            bool isObstacleMissing = IsObstacleMissing(ray, maxShootDistance);
            bool isTargetFound = isObstacleMissing;

            if (drawRay)
            {
                //Debug.DrawRay(eyesPosition, direction * maxShootDistance, Color.red);
                Debug.DrawRay(ray.origin, ray.direction * maxShootDistance, Color.red);
            }

            return isTargetFound;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsObstacleMissing(Ray ray, float maxDistance)
        {
            int hitsAmount = Physics.RaycastNonAlloc(ray, _hits, maxDistance, _aimObstaclesLayerMask);

            if (hitsAmount == 0)
                return false;

            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < hitsAmount; i++)
            {
                float distance = _hits[i].distance;

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestIndex = i;
                
            }

            int hitLayer = _hits[closestIndex].transform.gameObject.layer;
            bool isObstacle = _environmentLayerMask.Contains(hitLayer);

            if (isObstacle)
                return false;

            return true;
        }
    }
}