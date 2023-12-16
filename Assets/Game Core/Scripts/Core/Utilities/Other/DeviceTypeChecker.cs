using GameCore.Enums;
using UnityEngine;

namespace GameCore.Utilities
{
    public static class DeviceTypeChecker
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static DeviceName GetDeviceName()
        {
#if UNITY_IOS
            return GetIOSDeviceName();
#elif UNITY_ANDROID
            return GetAndroidDeviceName();
#else
            return DeviceName.Tablet;
#endif
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

#if UNITY_IOS
        private static DeviceName GetIOSDeviceName()
        {
            bool deviceIsIpad = Device.generation.ToString().Contains("iPad");

            if (deviceIsIpad)
                return DeviceName.Tablet;

            bool deviceIsIphone = Device.generation.ToString().Contains("iPhone");
            return deviceIsIphone ? DeviceName.Phone : DeviceName.Tablet;
        }
#endif

#if UNITY_ANDROID
        private static DeviceName GetAndroidDeviceName()
        {
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            float a = Mathf.Max(screenWidth, screenHeight);
            float b = Mathf.Min(screenWidth, screenHeight);
            
            float aspectRatio = a / b;
            bool isTablet = DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f;

            return isTablet ? DeviceName.Tablet : DeviceName.Phone;
        }
#endif

        private static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }
    }
}