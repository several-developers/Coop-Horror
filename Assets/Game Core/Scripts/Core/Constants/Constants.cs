using Unity.Netcode;

namespace GameCore
{
    public static class Constants
    {
        // FIELDS: --------------------------------------------------------------------------------

        public const string Empty = "Empty";
        public const string Settings = "Settings";
        public const string SettingsIn = "Settings/In";
        public const string References = "References";
        public const string DebugButtons = "Debug Buttons";
        public const string Visualizer = "Visualizer";
        public const string VisualizerIn = "Visualizer/In";
        public const string Animation = "Animation";
        public const string AnimationIn = "Animation/In";
        public const string Events = "Events";
        public const string DebugInfo = "Debug Info";

        public const string UISoundBus = "UI";

        public const string PlayerTag = "Player";
        
        public const string DefaultIP = "127.0.0.1";
        public const int DefaultPort = 9998;

        public const int HoursInDay = 24;
        public const int MinutesInDay = 1440;
        public const int SecondsInDay = 86400;
        public const int SecondsInHour = 3600;
        public const int SecondsInMinute = 60;

        public const int SaveCellsAmount = 3;
        public const int MaxPlayers = 4;
        public const int PlayerInventorySize = 6;

        public const NetworkVariableWritePermission OwnerPermission = NetworkVariableWritePermission.Owner;
    }
}