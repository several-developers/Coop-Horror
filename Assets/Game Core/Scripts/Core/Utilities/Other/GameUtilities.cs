using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.Utilities
{
    public static class GameUtilities
    {
        public static void ChangeCursorLockState(bool isLocked)
        {
            CursorLockMode cursorLockMode = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.lockState = cursorLockMode;
        }

        public static void SwapCursorLockState()
        {
            CursorLockMode cursorLockMode = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            
            Cursor.lockState = cursorLockMode;
        }

        public static void Teleport(Transform target, Transform parentFrom, Transform parentTo, out Vector3 position,
            out Quaternion rotation)
        {
            Vector3 targetPosition = target.position;
            Vector3 parent1Position = parentFrom.position;
            Vector3 parent2Position = parentTo.position;

            Quaternion parent1Rotation = parentFrom.rotation;
            Quaternion parent2Rotation = parentTo.rotation;

            Vector3 difference = targetPosition - parent1Position;
            Vector3 rotatedDifference = parent1Rotation * difference;
            position = parent2Position + parent2Rotation * rotatedDifference;

            Quaternion targetRotation = target.rotation;
            Vector3 rotationDifference = parent2Rotation.eulerAngles - parent1Rotation.eulerAngles;
            Vector3 eulerAngles = targetRotation.eulerAngles;
            eulerAngles.x += rotationDifference.x;
            eulerAngles.y += rotationDifference.y;
            rotation = Quaternion.Euler(eulerAngles);
        }

        public static bool DoSceneExist(string scene) =>
            Application.CanStreamedLevelBeLoaded(scene);

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