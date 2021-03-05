using System;
using System.Collections.Generic;

public static class MessagesAndErrors {
    public readonly struct ErrorMessage {
        /// <summary>
        /// ErrorCode to show (can be forwarded by customer support to developers)
        /// </summary>
        private readonly ErrorCode _errorCode;

        /// <summary>
        /// what has gone wrong.
        /// </summary>
        public readonly string ShortDescription;

        /// <summary>
        /// Possible causes and workarounds.
        /// </summary>
        public readonly string Description;

        public ErrorMessage(ErrorCode code, string shortDescription, string description) {
            _errorCode = code;
            ShortDescription = shortDescription;
            Description = description;
        }

        public override string ToString() {
            return $"Error Message {_errorCode}: {ShortDescription}{Environment.NewLine}{Description}";
        }
    }

    public readonly struct Message {
        /// <summary>
        /// What has happened.
        /// </summary>
        public readonly string ShortDescription;

        /// <summary>
        /// Detailed description what the backgrounds are.
        /// </summary>
        public readonly string Description;

        public Message(string shortDescription, string description) {
            ShortDescription = shortDescription;
            Description = description;
        }

        public override string ToString() {
            return $"Message: {ShortDescription}{Environment.NewLine}{Description}";
        }
    }

    public enum ErrorCode {
        OnPhotonCreateRoomFailed,
        OnBotInitFailed,

        // *** Photon connection fails (copied from enum PhotonNetworkingMessage)
        /// <summary>
        /// Called when a CreateRoom() call failed. Optional parameters provide ErrorCode and message.
        /// </summary>
        /// <remarks>
        /// Most likely because the room name is already in use (some other client was faster than you).
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        ///
        /// Example: void OnPhotonCreateRoomFailed() { ... }
        ///
        /// Example: void OnPhotonCreateRoomFailed(object[] codeAndMsg) { // codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.  }
        /// </remarks>
        Undefined,

        /// <summary>
        /// Called when a JoinRoom() call failed. Optional parameters provide ErrorCode and message.
        /// </summary>
        /// <remarks>
        /// Most likely error is that the room does not exist or the room is full (some other client was faster than you).
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        ///
        /// Example: void OnPhotonJoinRoomFailed() { ... }
        ///
        /// Example: void OnPhotonJoinRoomFailed(object[] codeAndMsg) { // codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.  }
        /// </remarks>
        OnPhotonJoinRoomFailed,

        /// <summary>(32758) Join can fail if the room (name) is not existing (anymore). This can happen when players leave while you join.</summary>
        OnPhotonJoinRoomFailedRoomDoesNotExist,

        ReConnectAndRejoinFailed,

        /// <summary>
        /// Called when something causes the connection to fail (after it was established), followed by a call to OnDisconnectedFromPhoton().
        /// </summary>
        /// <remarks>
        /// If the server could not be reached in the first place, OnFailedToConnectToPhoton is called instead.
        /// The reason for the error is provided as StatusCode.
        ///
        /// Example: void OnConnectionFail(DisconnectCause cause) { ... }
        /// </remarks>
        OnConnectionFail,

        /// <summary>
        /// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
        /// </summary>
        /// <remarks>
        /// OnConnectionFail only gets called when a connection to a Photon server was established in the first place.
        ///
        /// Example: void OnFailedToConnectToPhoton(DisconnectCause cause) { ... }
        /// </remarks>
        OnFailedToConnectToPhoton,

        /// <summary>
        /// Called after a JoinRandom() call failed. Optional parameters provide ErrorCode and message.
        /// </summary>
        /// <remarks>
        /// Most likely all rooms are full or no rooms are available.
        /// When using multiple lobbies (via JoinLobby or TypedLobby), another lobby might have more/fitting rooms.
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        ///
        /// Example: void OnPhotonRandomJoinFailed() { ... }
        ///
        /// Example: void OnPhotonRandomJoinFailed(object[] codeAndMsg) { // codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.  }
        /// </remarks>
        OnPhotonRandomJoinFailed,

        /// <summary>
        /// Because the concurrent user limit was (temporarily) reached, this client is rejected by the server and disconnecting.
        /// </summary>
        /// <remarks>
        /// When this happens, the user might try again later. You can't create or join rooms in OnPhotonMaxCcuReached(), cause the client will be disconnecting.
        /// You can raise the CCU limits with a new license (when you host yourself) or extended subscription (when using the Photon Cloud).
        /// The Photon Cloud will mail you when the CCU limit was reached. This is also visible in the Dashboard (webpage).
        ///
        /// Example: void OnPhotonMaxCcuReached() { ... }
        /// </remarks>
        OnPhotonMaxCcuReached,

        /// <summary>
        /// Called when the custom authentication failed. Followed by disconnect!
        /// </summary>
        /// <remarks>
        /// Custom Authentication can fail due to user-input, bad tokens/secrets.
        /// If authentication is successful, this method is not called. Implement OnJoinedLobby() or OnConnectedToMaster() (as usual).
        ///
        /// During development of a game, it might also fail due to wrong configuration on the server side.
        /// In those cases, logging the debugMessage is very important.
        ///
        /// Unless you setup a custom authentication service for your app (in the [Dashboard](https://www.photonengine.com/dashboard)),
        /// this won't be called!
        ///
        /// Example: void OnCustomAuthenticationFailed(string debugMessage) { ... }
        /// </remarks>
        OnCustomAuthenticationFailed,
    }

    private static readonly Dictionary<ErrorCode, ErrorMessage> _errorInformation =
        new Dictionary<ErrorCode, ErrorMessage> {
            {
                ErrorCode.Undefined,
                new ErrorMessage(ErrorCode.Undefined, "Undefined Error",
                    "This error is not defined, please send your logfile to customer support.")
            },
            {
                ErrorCode.OnBotInitFailed,
                new ErrorMessage(ErrorCode.OnBotInitFailed, "Load Bot Failed",
                    "Can't init Bot Instance!")
            },

            // *** Photon connection fails (copied from enum PhotonNetworkingMessage)
            {
                ErrorCode.OnPhotonCreateRoomFailed,
                new ErrorMessage(ErrorCode.OnPhotonCreateRoomFailed, "Create Room Failed", "Couldn't create a room.")
            }, {
                ErrorCode.OnPhotonJoinRoomFailed,
                new ErrorMessage(ErrorCode.OnPhotonJoinRoomFailed, "Join Room Failed", "Couldn't join a room.")
            }, {
                ErrorCode.OnPhotonJoinRoomFailedRoomDoesNotExist,
                new ErrorMessage(ErrorCode.OnPhotonJoinRoomFailedRoomDoesNotExist, "Join Room Failed",
                    "The room you want to join does not exist (please check your room name and if your admin created this room already). Will retry every " +
                    BitCompressionConstants.RetryJoinRoomTime + " seconds.")
            }, {
                ErrorCode.OnConnectionFail,
                new ErrorMessage(ErrorCode.OnConnectionFail, "Connection Fail", "Couldn't connect to a server.")
            }, {
                ErrorCode.OnFailedToConnectToPhoton,
                new ErrorMessage(ErrorCode.OnFailedToConnectToPhoton, "Failed To Connect To Photon",
                    "Couldn't connect to Photon.")
            }, {
                ErrorCode.OnPhotonRandomJoinFailed,
                new ErrorMessage(ErrorCode.OnPhotonRandomJoinFailed, "Photon Random Join Failed",
                    "Couldn't random join.")
            }, {
                ErrorCode.OnPhotonMaxCcuReached,
                new ErrorMessage(ErrorCode.OnPhotonMaxCcuReached, "Photon Max CCU Reached",
                    "Maximum CCU of Photon reached.")
            }, {
                ErrorCode.OnCustomAuthenticationFailed,
                new ErrorMessage(ErrorCode.OnCustomAuthenticationFailed, "Custom Authentication Failed",
                    "Couldn't authenticate.")
            }, {
                ErrorCode.ReConnectAndRejoinFailed,
                new ErrorMessage(ErrorCode.ReConnectAndRejoinFailed, "Reconnect Or Rejoin Failed",
                    "The reconnect or rejoin failed.")
            }
        };

    private static readonly Dictionary<ConnectionManager.ConnectionState, Message> _connectionInformation =
        new Dictionary<ConnectionManager.ConnectionState, Message>() {
            {
                ConnectionManager.ConnectionState.Undefined,
                new Message("Undefined Connection State",
                    "This connection state is not defined, please send your logfile to customer support.")
            }, {
                ConnectionManager.ConnectionState.Disconnected,
                new Message("Disconnected", "You're currently disconnected. Please connect to a Server.")
            }, {
                ConnectionManager.ConnectionState.Connecting,
                new Message("Connecting to a server", "Trying to connect to a server.")
            }, {
                ConnectionManager.ConnectionState.ConnectedToServer,
                new Message("Connected To Server",
                    "You're currently connected to a server. Start Matchmaking to enter a game room.")
            }, {
                ConnectionManager.ConnectionState.MatchMaking,
                new Message("Matchmaking In Progress",
                    "Matchmaking started. Please follow the instructions to enter a game room.")
            }, {
                ConnectionManager.ConnectionState.ConnectedToGame,
                new Message("Welcome", "You entered a game room. Have fun!")
            }
        };

    /// <summary>
    /// Show error messages to the user
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public static ErrorMessage GetErrorMessage(ErrorCode errorCode) {
        if (_errorInformation.ContainsKey(errorCode))
            return _errorInformation[errorCode];

        Debug.LogError($"Error: error code {errorCode} is not defined!");
        return _errorInformation[ErrorCode.Undefined];
    }

    // show messages while the user connects to the game
    public static Message GetConnectionMessage(ConnectionManager.ConnectionState connectionState) {
        if (_connectionInformation.ContainsKey(connectionState))
            return _connectionInformation[connectionState];

        Debug.LogError($"Error: no message for connection state {connectionState} defined!");
        return _connectionInformation[ConnectionManager.ConnectionState.Undefined];
    }
}