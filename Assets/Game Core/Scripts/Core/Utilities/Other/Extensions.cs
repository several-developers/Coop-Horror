using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Utilities
{
    public static class Extensions
    {
        public static Tweener VisibilityState(this CanvasGroup canvasGroup, bool show, float fadeTime = 0,
            bool ignoreScaleTime = false)
        {
            var canvasGroupTN = canvasGroup.DOFade(show ? 1 : 0, fadeTime).SetUpdate(ignoreScaleTime);
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;

            return canvasGroupTN;
        }

        public static void VisibilityState(this GameObject gameObject, bool show) =>
            gameObject.SetActive(show);

        public static Vector3 GetRandomPosition(this Transform transform, float radius = 1f)
        {
            if (radius <= 0.01f)
                return transform.position;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float rad = Random.Range(0f, radius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rad;
            
            return transform.position + offset;
        }
        
        /// <summary>
        /// Puts the string into the Clipboard.
        /// </summary>
        public static void CopyToClipboard(this string str) =>
            GUIUtility.systemCopyBuffer = str;

        public static string GetNiceName(this string text) =>
            Regex.Replace(text, "([a-z0-9])([A-Z0-9])", "$1 $2");
        
        public static string GetNiceName(this Enum text) =>
            Regex.Replace(text.ToString(), "([a-z0-9])([A-Z0-9])", "$1 $2");
        
        // Formatting number: 2530912 -> 2.530.912
        public static string FormatNumber(this int number) =>
            number.ToString("N0", CultureInfo.GetCultureInfo("de"));
            
        // Formatting number: 2530912 -> 2.530.912
        public static string FormatNumber(this float number) =>
            number.ToString("N0", CultureInfo.GetCultureInfo("de"));

        public static bool IsItemKeyOrIDValid(this string itemKey) =>
            !string.IsNullOrEmpty(itemKey);

        public static int ConvertToMilliseconds(this float value) =>
            Mathf.RoundToInt(value * 1000);

        public static void ConvertToMinutes(this float time, out string result)
        {
            time = Mathf.Max(time, 0);

            int min = Mathf.FloorToInt(time % 3600 / 60f);
            int sec = Mathf.FloorToInt(time % 60);

            result = $"{min:D2}:{sec:D2}";
        }

        public static void ConvertToHours(this float time, out string result)
        {
            time = Mathf.Max(time, 0);

            int hours = Mathf.FloorToInt(time / 3600f);
            int min = Mathf.FloorToInt(time % 3600 / 60f);
            int sec = Mathf.FloorToInt(time % 60);

            result = $"{hours:D2}:{min:D2}:{sec:D2}";
        }
        
        public static void ConvertToDateTimeNow(this int time, out string result)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(time);
            DateTime dateTime = dateTimeOffset.LocalDateTime;
            result = dateTime.ToString();
        }
        
        public static void ConvertToDateTimeUtc(this int time, out string result)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(time);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            result = dateTime.ToString();
        }
        
        public static float GetAnimationTime(this Animator animator, string animationName)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

            foreach (AnimationClip clip in clips)
                if (clip.name.Contains(animationName))
                    return clip.length;

            return 0;
        }
        
        public static bool Contains(this LayerMask mask, int layer) =>
            ((mask & (1 << layer)) != 0);
    }
}