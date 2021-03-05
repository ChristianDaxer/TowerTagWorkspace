using System;
using System.Collections;
using GameManagement;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI {
    /// <summary>
    /// Controller for the overlay message canvas.
    /// Makes the canvas display messages by working through the overlay message queue.
    /// Also reacts to some
    /// </summary>
    [RequireComponent(typeof(ConnectionManager))]
    public class OverlayUiController : MonoBehaviour {
        [SerializeField, Tooltip("The message queue for overlay display messages and errors")]
        private MessageQueue _messageQueue;

        [SerializeField,
         Tooltip("Set the number of seconds until the overlay disappears when the player is connected to a game")]
        private float _hideOverlayWhenConnectedTimeout = 3f;

        [SerializeField, Tooltip("Drag the Overlay Canvas Prefab here")]
        private OverlayCanvasModel _screenMessageCanvasPrefab;

        [SerializeField, Tooltip("Drag the VR message Canvas Prefab here")]
        private OverlayCanvasModel _VRMessageCanvasPrefab;

        private ConnectionManagerHome _connectionManagerHome;

        private Message _currentMessage;
        private OverlayCanvasModel _overlayCanvas;
        private Coroutine _autoCloseCoroutine;
        private bool ConfirmationRequired => _currentMessage != null && _currentMessage.NeedsConfirmation;

        private OverlayCanvasModel OverlayCanvas {
            get {
                if (_overlayCanvas != null) return _overlayCanvas;

                return CreateCanvas();
            }
        }

        private OverlayCanvasModel CreateCanvas()
        {
            _overlayCanvas =
                InstantiateWrapper.InstantiateWithMessage(TowerTagSettings.Home && !SharedControllerType.Spectator
                    ? _VRMessageCanvasPrefab
                    : _screenMessageCanvasPrefab);
            DontDestroyOnLoad(OverlayCanvas);
            _overlayCanvas.OnClose += OnCloseCanvas;
            _overlayCanvas.Hide();
            return _overlayCanvas;
        }


        private void OnEnable() {
            ConnectionManager.Instance.ConnectionStateChanged += OnConnectionStateChanged;
            ConnectionManager.Instance.ErrorOccured += OnErrorOccurred;
            GameManager.Instance.MatchHasFinishedLoading += OnMatchLoaded;
            _messageQueue.MessageAdded += DisplayNextMessageAdded;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void Start() {
            if (TowerTagSettings.Home) {
                try {
                    _connectionManagerHome = FindObjectOfType<ConnectionManagerHome>();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
                finally {
                    if (_connectionManagerHome != null)
                        _connectionManagerHome.ErrorOccured += OnErrorOccurred;
                }
            }

            StartCoroutine(InitCanvas());
        }

        private IEnumerator InitCanvas()
        {
            if(!GameInitialization.Initialized)
                yield return new WaitUntil(() => GameInitialization.Initialized);
            CreateCanvas();
        }

        private void OnSceneChanged(Scene sceneA, Scene sceneB) {
            if (TTSceneManager.Instance != null && TTSceneManager.Instance.IsInHubScene) {
                if (_currentMessage != null && !ConfirmationRequired) {
                    CloseCurrentMessage();
                    _currentMessage = _messageQueue.GetNextMessage();
                }

                DisplayCurrentMessage();
            }
        }

        /// <summary>
        /// Close all volatile messages, keep the first one that needs confirmation, or the last one in the queue.
        /// If the current message needs confirmation, it will not be closed.
        /// </summary>
        private void DisplayNextMessageAdded() {
            while (_messageQueue.HasNext() && !ConfirmationRequired) {
                CloseCurrentMessage();
                _currentMessage = _messageQueue.GetNextMessage();
            }

            DisplayCurrentMessage();
        }

        private void OnMatchLoaded(IMatch match) {
            if (_currentMessage != null && !ConfirmationRequired) {
                CloseCurrentMessage();
                _currentMessage = _messageQueue.GetNextMessage();
            }

            DisplayCurrentMessage();
        }

        /// <summary>
        /// Handle when user closes the message overlay
        /// </summary>
        private void OnCloseCanvas() {
            CloseCurrentMessage();
            DisplayNextMessageAdded();
        }

        /// <summary>
        /// Close the current message, invoking the respective callbacks. Does not hide the canvas.
        /// </summary>
        private void CloseCurrentMessage() {
            _currentMessage?.Closed?.Invoke();
            _currentMessage = null;
            if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
            _autoCloseCoroutine = null;
        }

        /// <summary>
        /// Display the current message, invoking its on open callback. If there is no message, hide the overlay canvas.
        /// </summary>
        private void DisplayCurrentMessage() {
            if (_currentMessage == null) {
                OverlayCanvas.Hide();
                return;
            }

            _currentMessage.Opened?.Invoke();

            OverlayCanvas.ShowMessage(
                _currentMessage.Header,
                _currentMessage.Text,
                _currentMessage.Buttons.Count == 0 || _currentMessage.NeedsConfirmation,
                _currentMessage.Buttons,
                _currentMessage.InputFields);

            if (_currentMessage.LifeTime > 0) {
                _autoCloseCoroutine = StartCoroutine(AutoClose(_currentMessage));
            }
        }

        private IEnumerator AutoClose(Message message) {
            yield return new WaitForSeconds(message.LifeTime);

            CloseCurrentMessage();
            DisplayNextMessageAdded();
        }

        /// <summary>
        /// React to connection changes: If connected to game, the current message should be closed with a delay.
        /// </summary>
        private void OnConnectionStateChanged(ConnectionManager connectionManager,
            ConnectionManager.ConnectionState previousState, ConnectionManager.ConnectionState newState) {
            // This method was always returning, so I've commented out all of it so it stops throwing unreachable code.
            /*
            // if (previousState == ConnectionManager.ConnectionState.Undefined)
                return; // don't show disconnect popup on startup
            MessagesAndErrors.Message connectionMessage = MessagesAndErrors.GetConnectionMessage(newState);

            bool enableMessage;
            switch (newState) {
                case ConnectionManager.ConnectionState.Connecting:
                    if (TowerTagSettings.Home && MySceneManager.Instance.IsInLicensingScene)
                        enableMessage = false;
                    else
                        enableMessage = true;
                    break;
                case ConnectionManager.ConnectionState.Disconnected:
                    enableMessage = false;
                    break;
                case ConnectionManager.ConnectionState.Undefined:
                    enableMessage = true;
                    break;
                case ConnectionManager.ConnectionState.MatchMaking:
                    enableMessage = false;
                    break;
                case ConnectionManager.ConnectionState.ConnectedToGame:
                    enableMessage = true;
                    break;
                case ConnectionManager.ConnectionState.ConnectedToServer:
                    enableMessage = false;
                    break;
                default:
                    enableMessage = false;
                    break;
            }

            if (enableMessage) {
                _messageQueue.AddVolatileMessage(connectionMessage.Description,
                    connectionMessage.ShortDescription,
                    null,
                    _currentMessage?.Closed,
                    null,
                    5);
            }

            if (newState == ConnectionManager.ConnectionState.ConnectedToGame) {
                StartCoroutine(CloseMessageWithDelay());
            }
            */
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
                _currentMessage?.Buttons[0]?.Clicked?.Invoke();
                _overlayCanvas.Close();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && _currentMessage != null) {
                _overlayCanvas.Close();
            }
        }

        /// <summary>
        /// Closes all volatile messages after a delay. If there are none, this will close the overlay.
        /// </summary>
        private IEnumerator CloseMessageWithDelay() {
            yield return new WaitForSeconds(_hideOverlayWhenConnectedTimeout);

            while (_currentMessage != null && !ConfirmationRequired) {
                CloseCurrentMessage();
                _currentMessage = _messageQueue.GetNextMessage();
            }

            DisplayCurrentMessage();
        }

        private void OnErrorOccurred(ConnectionManagerHome connectionManager, MessagesAndErrors.ErrorCode errorCode) {
            DisplayErrorMessage(errorCode);
        }

        /// <summary>
        /// React to errors by displaying the respective message. Errors must be confirmed.
        /// </summary>
        private void OnErrorOccurred(ConnectionManager connectionManager, MessagesAndErrors.ErrorCode errorCode) {
            DisplayErrorMessage(errorCode);
        }

        private void DisplayErrorMessage(MessagesAndErrors.ErrorCode errorCode) {
            MessagesAndErrors.ErrorMessage errorMessage = MessagesAndErrors.GetErrorMessage(errorCode);
            bool enableMessage;
            switch (errorCode) {
                case MessagesAndErrors.ErrorCode.Undefined:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnBotInitFailed:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnConnectionFail:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnCustomAuthenticationFailed:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnPhotonCreateRoomFailed:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnPhotonJoinRoomFailed:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnPhotonMaxCcuReached:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnPhotonRandomJoinFailed:
                    enableMessage = false;
                    break;
                case MessagesAndErrors.ErrorCode.ReConnectAndRejoinFailed:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnFailedToConnectToPhoton:
                    enableMessage = true;
                    break;
                case MessagesAndErrors.ErrorCode.OnPhotonJoinRoomFailedRoomDoesNotExist:
                    enableMessage = true;
                    break;
                default:
                    enableMessage = false;
                    break;
            }

            if (enableMessage) {
                _messageQueue.AddErrorMessage(errorMessage.Description,
                    errorMessage.ShortDescription);
            }
        }

        private void OnDisable() {
            if (_overlayCanvas != null) _overlayCanvas.OnClose -= OnCloseCanvas;
            if (_messageQueue != null) _messageQueue.MessageAdded -= DisplayNextMessageAdded;
            if (ConnectionManager.Instance != null) {
                ConnectionManager.Instance.ConnectionStateChanged -= OnConnectionStateChanged;
                ConnectionManager.Instance.ErrorOccured -= OnErrorOccurred;
            }

            GameManager.Instance.MatchHasFinishedLoading -= OnMatchLoaded;
            SceneManager.activeSceneChanged -= OnSceneChanged;

            if (TowerTagSettings.Home && _connectionManagerHome != null)
                _connectionManagerHome.ErrorOccured -= OnErrorOccurred;
        }
    }
}