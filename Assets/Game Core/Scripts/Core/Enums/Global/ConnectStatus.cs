namespace GameCore.Enums.Global
{
    public enum ConnectStatus
    {
        Undefined = 0,
        Success = 1,                  // Client successfully connected. This may also be a successful reconnect.
        ServerFull = 2,               // Can't join, server is already at capacity.
        LoggedInAgain = 3,            // Logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect = 4,  // Intentional Disconnect triggered by the user.
        GenericDisconnect = 5,        // Server disconnected, but no specific reason given.
        Reconnecting = 6,             // Client lost connection and is attempting to reconnect.
        IncompatibleBuildType = 7,    // Client build type is incompatible with server.
        HostEndedSession = 8,         // Host intentionally ended the session.
        StartHostFailed = 9,          // Server failed to bind.
        StartClientFailed = 10        // Failed to connect to server and/or invalid network endpoint.
    }
}