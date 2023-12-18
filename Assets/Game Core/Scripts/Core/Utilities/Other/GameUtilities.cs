using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.Utilities
{
    public static class GameUtilities
    {
        public static void ChangeCursorLockState()
        {
            CursorLockMode lockState = Cursor.lockState;

            Cursor.lockState = lockState switch
            {
                CursorLockMode.None => CursorLockMode.Locked,
                CursorLockMode.Locked => CursorLockMode.None,
                _ => Cursor.lockState
            };
        }

        // Get Mouse Position in World with Z = 0f
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        public static Vector3 GetMouseWorldPositionWithZ() =>
            GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);

        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera) =>
            GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera) =>
            worldCamera.ScreenToWorldPoint(screenPosition);

        public static Vector3 GetDirToMouse(Vector3 fromPosition)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            return (mouseWorldPosition - fromPosition).normalized;
        }

        // Is Mouse over a UI Element? Used for ignoring World clicks through UI
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            PointerEventData eventData = new(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> hits = new();
            EventSystem.current.RaycastAll(eventData, hits);

            return hits.Count > 0;
        }

        // Generate random normalized direction
        public static Vector3 GetRandomDir() =>
            new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        // Generate random normalized direction
        public static Vector3 GetRandomDirXZ() =>
            new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;


        public static Vector3 GetVectorFromAngle(int angle)
        {
            // angle = 0 -> 360
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static Vector3 GetVectorFromAngle(float angle)
        {
            // angle = 0 -> 360
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static Vector3 GetVectorFromAngleInt(int angle)
        {
            // angle = 0 -> 360
            float angleRad = angle * (Mathf.PI / 180f);
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static float GetAngleFromVectorFloat(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (n < 0)
                n += 360;

            return n;
        }

        public static float GetAngleFromVectorFloatXZ(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

            if (n < 0)
                n += 360;

            return n;
        }

        public static int GetAngleFromVector(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (n < 0)
                n += 360;

            int angle = Mathf.RoundToInt(n);

            return angle;
        }

        public static int GetAngleFromVector180(Vector3 dir)
        {
            dir = dir.normalized;
            float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            int angle = Mathf.RoundToInt(n);

            return angle;
        }

        public static Vector3 ApplyRotationToVector(Vector3 vec, Vector3 vecRotation) =>
            ApplyRotationToVector(vec, GetAngleFromVectorFloat(vecRotation));

        public static Vector3 ApplyRotationToVector(Vector3 vec, float angle) =>
            Quaternion.Euler(0, 0, angle) * vec;

        public static Vector3 ApplyRotationToVectorXZ(Vector3 vec, float angle) =>
            Quaternion.Euler(0, angle, 0) * vec;
    }
}