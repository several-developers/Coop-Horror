using System;
using GameCore.Gameplay.Entities.Player.Other;
using GameCore.Gameplay.Locations.GameTime;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public partial class TheNetworkHorror
    {
        private class ServerLogic
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ServerLogic(TheNetworkHorror networkHorror) =>
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
            }
            
            public void Dispose()
            {
                
            }

            public void Update()
            {
     
            }

            public void UpdateGameTimer()
            {
                DateTime dateTime = _networkHorror._timeCycleDecorator.GetDateTime();
                MyDateTime myDateTime = new(dateTime.Second, dateTime.Minute, dateTime.Hour);
                _networkHorror._gameTimer.Value = myDateTime;
            }

            // EVENTS RECEIVERS: ----------------------------------------------------------------------
            
        }
    }
}