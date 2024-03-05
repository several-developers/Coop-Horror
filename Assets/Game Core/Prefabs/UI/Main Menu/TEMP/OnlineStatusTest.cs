using System;
using GameCore.Enums.Global;
using GameCore.Gameplay.Network.ConnectionManagement;
using GameCore.Gameplay.PubSub;
using UnityEngine;
using Zenject;

namespace GameCore.UI.MainMenu.Temp
{
    public class OnlineStatusTest : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ISubscriber<ConnectStatus> connectStatusSubscriber,
            ISubscriber<ReconnectMessage> reconnectMessageSubscriber)
        {
            _reconnectMessageSubscriber = reconnectMessageSubscriber;
            _connectStatusSubscriber = connectStatusSubscriber;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private ISubscriber<ConnectStatus> _connectStatusSubscriber;
        private ISubscriber<ReconnectMessage> _reconnectMessageSubscriber;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _connectStatusSubscriber.Subscribe(OnConnectStatusChanged);
            _reconnectMessageSubscriber.Subscribe(OnReconnectMessageChanged);
        }

        private void OnDestroy()
        {
            _connectStatusSubscriber.Unsubscribe(OnConnectStatusChanged);
            _reconnectMessageSubscriber.Unsubscribe(OnReconnectMessageChanged);
        }

        private void LogConnectStatus(ConnectStatus connectStatus)
        {
             switch (connectStatus)
            {
                case ConnectStatus.Undefined:
                case ConnectStatus.UserRequestedDisconnect:
                    break;
                
                case ConnectStatus.ServerFull:
                    Debug.Log("Connection Failed" + "The Host is full and cannot accept any additional connections.");
                    break;
                
                case ConnectStatus.Success:
                    break;
                
                case ConnectStatus.LoggedInAgain:
                    Debug.Log("Connection Failed" + "You have logged in elsewhere using the same account. If you still want to connect, select a different profile by using the 'Change Profile' button.");
                    break;
                
                case ConnectStatus.IncompatibleBuildType:
                    Debug.Log("Connection Failed" + "Server and client builds are not compatible. You cannot connect a release build to a development build or an in-editor session.");
                    break;
                
                case ConnectStatus.GenericDisconnect:
                    Debug.Log("Disconnected From Host" + "The connection to the host was lost.");
                    break;
                
                case ConnectStatus.HostEndedSession:
                    Debug.Log("Disconnected From Host" + "The host has ended the game session.");
                    break;
                
                case ConnectStatus.Reconnecting:
                    break;
                
                case ConnectStatus.StartHostFailed:
                    Debug.Log("Connection Failed" + "Starting host failed.");
                    break;
                
                case ConnectStatus.StartClientFailed:
                    Debug.Log("Connection Failed" + "Starting client failed.");
                    break;
                
                default:
                    Debug.LogWarning($"New ConnectStatus {connectStatus} has been added, but no connect message defined for it.");
                    break;
            }
        }

        private void LogReconnectMessage(ReconnectMessage reconnectMessage)
        {
            if (reconnectMessage.CurrentAttempt == reconnectMessage.MaxAttempt)
            {
                Debug.Log("Close reconnect popup");
            }
            else
            {
                Debug.Log("Connection lost" + 
                          $"Attempting to reconnect...\nAttempt {reconnectMessage.CurrentAttempt + 1}/{reconnectMessage.MaxAttempt}");
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnConnectStatusChanged(ConnectStatus connectStatus) => LogConnectStatus(connectStatus);

        private void OnReconnectMessageChanged(ReconnectMessage reconnectMessage) =>
            LogReconnectMessage(reconnectMessage);
    }
}
