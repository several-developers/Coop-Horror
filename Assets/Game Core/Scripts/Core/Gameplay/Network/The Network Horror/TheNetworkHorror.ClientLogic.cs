using GameCore.Gameplay.Locations.GameTime;

namespace GameCore.Gameplay.Network
{
    public partial class TheNetworkHorror
    {
        private class ClientLogic
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ClientLogic(TheNetworkHorror networkHorror) =>
                _networkHorror = networkHorror;

            // FIELDS: --------------------------------------------------------------------------------
            
            private readonly TheNetworkHorror _networkHorror;
            
            private bool _isInitialized;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public void Init()
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;
                
                _networkHorror._gameTimer.OnValueChanged += OnGameTimerUpdated;
            }

            public void Dispose()
            {
                _networkHorror._gameTimer.OnValueChanged -= OnGameTimerUpdated;
            }
            
            public void Update()
            {
                
            }

            // EVENTS RECEIVERS: ----------------------------------------------------------------------

            private void OnGameTimerUpdated(MyDateTime previousDate, MyDateTime newDate) =>
                _networkHorror._timeCycleDecorator.SyncDateTime(newDate);
        }
    }
}