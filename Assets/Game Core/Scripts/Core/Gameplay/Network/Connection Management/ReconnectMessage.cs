namespace GameCore.Gameplay.Network.ConnectionManagement
{
    public struct ReconnectMessage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public ReconnectMessage(int currentAttempt, int maxAttempt)
        {
            CurrentAttempt = currentAttempt;
            MaxAttempt = maxAttempt;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public int CurrentAttempt;
        public int MaxAttempt;
    }
}