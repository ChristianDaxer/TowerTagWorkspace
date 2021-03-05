using System;
using JetBrains.Annotations;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
//using Valve.VR;

public class BadaboomHyperactionInputModule : PointerInputModule {
    public Hand _hand = Hand.LeftHand;

   // public SteamVR_Action_Boolean _triggerAction;
    private Transform _controllerLeft, _controllerRight;

    public PlayerInputBase XRController_Left, XRController_Right = null;

    [Header("Other Settings")] private float _prevMouseActionTime;
    private Vector2 _lastMouseMoveVector;
    private int _consecutiveMouseMoveCount;
    [SerializeField] private string _horizontalAxis = "Horizontal";
    [SerializeField] private string _verticalAxis = "Vertical";
    [SerializeField] private float _repeatDelay = 0.5f;
    [SerializeField] private float _mouseInputActionsPerSecond = 10;

    /// <summary>
    /// Name of the submit button.
    /// </summary>
    [SerializeField] private string _submitButton = "Submit";

    /// <summary>
    /// Name of the submit button.
    /// </summary>
    [SerializeField] private string _cancelButton = "Cancel";

    // Properties
    public Transform TargetControllerTransform {
        get {
            if (_hand == Hand.LeftHand) {
                return _controllerLeft;
            }

            return _controllerRight;
        }
    }

    [SerializeField] private Camera _helperCamera;
    [FormerlySerializedAs("_controller")] [SerializeField] private BadaboomHyperactionPointer _pointer;

    // Support variables
    private bool _triggerPressed;
    private bool _triggerPressedLastFrame;
    private PointerEventData _pointerEventData;
    [SerializeField] private Vector3 _lastRaycastHitPoint;
    private float _pressedDistance; // Distance the laser travelled while pressed.
    private PlayerRigBase playerRigBase;

    protected override void OnEnable() {
        base.OnEnable();
        ReadyTowerUiController.ReadyTowerUiInstantiated += OnReadyTowerUiControllerInstantiated;
    }

    protected override void OnDisable() {
        base.OnDisable();
        ReadyTowerUiController.ReadyTowerUiInstantiated -= OnReadyTowerUiControllerInstantiated;
    }

    protected override void Start() {
        base.Start();
        DontDestroyOnLoad(this);
        // Create a helper camera that will be used for ray casts
        _helperCamera = new GameObject("Helper Camera").AddComponent<Camera>();
        _helperCamera.transform.parent = this.transform;
        // Add physics raycaster for 3d objects
        _helperCamera.gameObject.AddComponent<PhysicsRaycaster>();
        _helperCamera.cullingMask = 0;
        _helperCamera.clearFlags = CameraClearFlags.Nothing;
        _helperCamera.nearClipPlane = 0.01f;
        _helperCamera.enabled = false;
        InitPlayArea();
        // Detect scene change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnReadyTowerUiControllerInstantiated(ReadyTowerUiController sender) {
        sender.GetComponentInChildren<Canvas>().worldCamera = _helperCamera;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        InitPlayArea();
    }

    private void InitPlayArea() {
        SetCanvasCamera();

        if (playerRigBase == null)
        {
            if (!PlayerRigBase.GetInstance(out playerRigBase))
                return;
        }

        SetupHmd(playerRigBase);
    }

    private void SetCanvasCamera() {
        if (null != _helperCamera) {
            // Assign all the canvases with the helper camera;

            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (var canvas in canvases) {
                // check if gameObject is a prefab
                if (canvas.gameObject.scene.name == null)
                    continue;
                canvas.worldCamera = _helperCamera;
            }
        }
    }

    /// <summary>
    /// Old version
    /// </summary>
    /// <param name="steamVRPlayArea"></param>
    //[Obsolete]
    //public void SetupHmd([NotNull] SteamVR_PlayArea steamVRPlayArea) {
    //    if (steamVRPlayArea == null) throw new ArgumentNullException(nameof(steamVRPlayArea));
    //    SetupControllers(steamVRPlayArea);
    //    if (null == _triggerAction) {
    //        Debug.LogError("No trigger action assigned");
    //    }
    //}

    public void SetController(BadaboomHyperactionPointer controller) {
        _pointer = controller;
    }

    public void RemoveController(BadaboomHyperactionPointer controller) {
        if (null != _pointer && _pointer == controller) {
            _pointer = null;
        }
    }

    #region PlayerInput
    public void SetupHmd([NotNull] PlayerRigBase playArea)
    {
        Debug.Log("SetupHmd PlayerRigBase");
        if (playArea == null) throw new ArgumentNullException(nameof(playArea));
        SetupControllers(playArea);
    }

    #endregion


    public override void Process() {
        if (_pointer != null) {
            UpdateHelperCamera();
            CheckTriggerStatus();
            ProcessLaserPointer();
        }

        if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
            return;

        bool usedEvent = SendUpdateEventToSelectedObject();

        // case 1004066 - touch / mouse events should be processed before navigation events in case
        // they change the current selected game object and the submit button is a touch / mouse button.

        // touch needs to take precedence because of the mouse emulation layer
        if (!ProcessTouchEvents() && input.mousePresent)
            ProcessMouseEvent();

        if (eventSystem.sendNavigationEvents) {
            if (!usedEvent)
                usedEvent |= SendMoveEventToSelectedObject();

            if (!usedEvent)
                SendSubmitEventToSelectedObject();
        }
    }

    /// <summary>
    /// Calculate and send a move event to the current selected object.
    /// </summary>
    /// <returns>If the move event was used by the selected object.</returns>
    private bool SendMoveEventToSelectedObject() {
        float time = Time.unscaledTime;

        Vector2 movement = GetRawMoveVector();
        if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f)) {
            _consecutiveMouseMoveCount = 0;
            return false;
        }

        bool similarDir = (Vector2.Dot(movement, _lastMouseMoveVector) > 0);

        // If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
        if (similarDir && _consecutiveMouseMoveCount == 1) {
            if (time <= _prevMouseActionTime + _repeatDelay)
                return false;
        }
        // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
        else {
            if (time <= _prevMouseActionTime + 1f / _mouseInputActionsPerSecond)
                return false;
        }

        var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);

        if (axisEventData.moveDir != MoveDirection.None) {
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            if (!similarDir)
                _consecutiveMouseMoveCount = 0;
            _consecutiveMouseMoveCount++;
            _prevMouseActionTime = time;
            _lastMouseMoveVector = movement;
        }
        else {
            _consecutiveMouseMoveCount = 0;
        }

        return axisEventData.used;
    }

    private Vector2 GetRawMoveVector() {
        Vector2 move = Vector2.zero;
        move.x = input.GetAxisRaw(_horizontalAxis);
        move.y = input.GetAxisRaw(_verticalAxis);

        if (input.GetButtonDown(_horizontalAxis)) {
            if (move.x < 0)
                move.x = -1f;
            if (move.x > 0)
                move.x = 1f;
        }

        if (input.GetButtonDown(_verticalAxis)) {
            if (move.y < 0)
                move.y = -1f;
            if (move.y > 0)
                move.y = 1f;
        }

        return move;
    }

    private void ProcessMouseEvent() {
        ProcessMouseEvent(0);
    }

    /// <summary>
    /// Process all mouse events.
    /// </summary>
    private void ProcessMouseEvent(int id) {
        var mouseData = GetMousePointerEventData(id);
        var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        // Process the first mouse button fully
        ProcessMousePress(leftButtonData);
        ProcessMove(leftButtonData.buttonData);
        ProcessDrag(leftButtonData.buttonData);

        // Now process right / middle clicks
        ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
        ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

        if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f)) {
            var scrollHandler =
                ExecuteEvents.GetEventHandler<IScrollHandler>(
                    leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
        }
    }

    /// <summary>
    /// Calculate and process any mouse button state changes.
    /// </summary>
    private void ProcessMousePress(MouseButtonEventData data) {
        var pointerEvent = data.buttonData;
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        // PointerDown notification
        if (data.PressedThisFrame()) {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed =
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // Debug.Log("Pressed: " + newPressed);

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress) {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;

                pointerEvent.clickTime = time;
            }
            else {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;

            pointerEvent.clickTime = time;

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        // PointerUp notification
        if (data.ReleasedThisFrame()) {
            ReleaseMouse(pointerEvent, currentOverGo);
        }
    }

    private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo) {
        ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

        var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

        // PointerClick and Drop events
        if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
        }
        else if (pointerEvent.pointerDrag != null && pointerEvent.dragging) {
            ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
        }

        pointerEvent.eligibleForClick = false;
        pointerEvent.pointerPress = null;
        pointerEvent.rawPointerPress = null;

        if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

        pointerEvent.dragging = false;
        pointerEvent.pointerDrag = null;

        // redo pointer enter / exit to refresh state
        // so that if we moused over something that ignored it before
        // due to having pressed on something else
        // it now gets it.
        if (currentOverGo != pointerEvent.pointerEnter) {
            HandlePointerExitAndEnter(pointerEvent, null);
            HandlePointerExitAndEnter(pointerEvent, currentOverGo);
        }
    }

    private bool ProcessTouchEvents() {
        for (var i = 0; i < input.touchCount; ++i) {
            Touch touch = input.GetTouch(i);

            if (touch.type == TouchType.Indirect)
                continue;

            PointerEventData pointer = GetTouchPointerEventData(touch, out bool pressed, out bool released);

            ProcessTouchPress(pointer, pressed, released);

            if (!released) {
                ProcessMove(pointer);
                ProcessDrag(pointer);
            }
            else
                RemovePointerData(pointer);
        }

        return input.touchCount > 0;
    }

    /// <summary>
    /// This method is called by Unity whenever a touch event is processed. Override this method with a custom implementation to process touch events yourself.
    /// </summary>
    /// <param name="pointerEvent">Event data relating to the touch event, such as position and ID to be passed to the touch event destination object.</param>
    /// <param name="pressed">This is true for the first frame of a touch event, and false thereafter. This can therefore be used to determine the instant a touch event occurred.</param>
    /// <param name="released">This is true only for the last frame of a touch event.</param>
    /// <remarks>
    /// This method can be overridden in derived classes to change how touch press events are handled.
    /// </remarks>
    private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released) {
        var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

        // PointerDown notification
        if (pressed) {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            if (pointerEvent.pointerEnter != currentOverGo) {
                // send a pointer enter to the touched element if it isn't the one to select...
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }

            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed =
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // Debug.Log("Pressed: " + newPressed);

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress) {
                var diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;

                pointerEvent.clickTime = time;
            }
            else {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;

            pointerEvent.clickTime = time;

            // Save the drag handler as well
            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        // PointerUp notification
        if (released) {
            // Debug.Log("Executing press up on: " + pointer.pointerPress);
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

            // see if we mouse up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            else if (pointerEvent.pointerDrag != null && pointerEvent.dragging) {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            // send exit events as we need to simulate this on touch up on touch device
            ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
            pointerEvent.pointerEnter = null;
        }
    }

    /// <summary>
    /// Calculate and send a submit event to the current selected object.
    /// </summary>
    /// <returns>If the submit event was used by the selected object.</returns>
    private bool SendSubmitEventToSelectedObject() {
        if (eventSystem.currentSelectedGameObject == null)
            return false;

        var data = GetBaseEventData();
        if (input.GetButtonDown(_submitButton))
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

        if (input.GetButtonDown(_cancelButton))
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
        return data.used;
    }

    // Update helper camera position
    private void UpdateHelperCamera() {
        Transform helperCameraTransform = _helperCamera.transform;
        Transform controllerTransform = _pointer.transform;
        helperCameraTransform.position = controllerTransform.position;
        helperCameraTransform.rotation = controllerTransform.rotation;
    }

    private void CheckTriggerStatus() {
        if (SharedControllerType.VR) {
            // Using the action system
            if (_hand == Hand.LeftHand) {
               // _triggerPressed = _triggerAction.GetState(SteamVR_Input_Sources.LeftHand);
                _triggerPressed = XRController_Left.TriggerHold;
            }
            else if (_hand == Hand.RightHand) {
               // _triggerPressed = _triggerAction.GetState(SteamVR_Input_Sources.RightHand);
                _triggerPressed = XRController_Right.TriggerHold;
            }
        }

        if (SharedControllerType.NormalFPS) {
            _triggerPressed = Input.GetMouseButton(0);
        }
    }

    private void ProcessLaserPointer() {
        SendUpdateEventToSelectedObject();

        PointerEventData eventData = GetPointerEventData();
        ProcessPress(eventData);
        ProcessMove(eventData);
        
        if (_triggerPressed) {
            

            ProcessVRDrag(eventData);

            if (!Mathf.Approximately(eventData.scrollDelta.sqrMagnitude, 0.0f)) {
                var scrollHandler =
                    ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
            }
        }

        _triggerPressedLastFrame = _triggerPressed;
    }

    protected override void ProcessMove(PointerEventData pointerEvent) {
        var targetGo = pointerEvent.pointerCurrentRaycast.gameObject;
        HandlePointerExitAndEnter(pointerEvent, targetGo);
    }

    private void ProcessPress(PointerEventData eventData) {
        GameObject currentOverGo = eventData.pointerCurrentRaycast.gameObject;

        // PointerDown notification
        if (TriggerPressedThisFrame()) {
            eventData.eligibleForClick = true;
            eventData.delta = Vector2.zero;
            eventData.dragging = false;
            eventData.useDragThreshold = true;
            eventData.pressPosition = eventData.position;
            eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
            _pressedDistance = 0;

            if (eventData.pointerEnter != currentOverGo) {
                // send a pointer enter to the touched element if it isn't the one to select...
                HandlePointerExitAndEnter(eventData, currentOverGo);
                eventData.pointerEnter = currentOverGo;
            }

            // search for the control that will receive the press
            // if we can't find a press handler set the press
            // handler to be what would receive a click.
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.pointerDownHandler);

            // didnt find a press handler... search for a click handler
            if (newPressed == null)
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            float time = Time.unscaledTime;

            if (newPressed == eventData.lastPress) {
                var diffTime = time - eventData.clickTime;
                if (diffTime < 0.3f)
                    ++eventData.clickCount;
                else
                    eventData.clickCount = 1;

                eventData.clickTime = time;
            }
            else {
                eventData.clickCount = 1;
            }

            eventData.pointerPress = newPressed;
            eventData.rawPointerPress = currentOverGo;

            eventData.clickTime = time;

            // Save the drag handler as well
            eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (eventData.pointerDrag != null)
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
        }

        // PointerUp notification
        if (TriggerReleasedThisFrame()) {
            ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

            // see if we button up on the same element that we clicked on...
            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (eventData.pointerPress == pointerUpHandler && eventData.eligibleForClick) {
                ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
            }
            else if (eventData.pointerDrag != null && eventData.dragging) {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, eventData, ExecuteEvents.dropHandler);
            }

            eventData.eligibleForClick = false;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;
            _pressedDistance = 0;

            if (eventData.pointerDrag != null && eventData.dragging) {
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
            }

            eventData.dragging = false;
            eventData.pointerDrag = null;

            // send exit events as we need to simulate this on touch up on touch device
            ExecuteEvents.ExecuteHierarchy(eventData.pointerEnter, eventData, ExecuteEvents.pointerExitHandler);
            eventData.pointerEnter = null;
        }
    }

    private PointerEventData GetPointerEventData() {
        if (null == _pointerEventData) {
            _pointerEventData = new PointerEventData(eventSystem);
        }

        _pointerEventData.Reset();
        _pointerEventData.position = new Vector2(_helperCamera.pixelWidth / 2f,
            _helperCamera.pixelHeight / 2f);
        _pointerEventData.scrollDelta = Vector2.zero;

        eventSystem.RaycastAll(_pointerEventData, m_RaycastResultCache);
        RaycastResult currentRaycast = FindFirstRaycast(m_RaycastResultCache);
        _pointerEventData.pointerCurrentRaycast = currentRaycast;

        // Delta is used to define if the cursor was moved.
        // It will be used for drag threshold calculation, which we'll calculate angle in degrees
        // between the last and the current raycasts.
        Transform helperCameraTransform = _helperCamera.transform;
        var ray = new Ray(helperCameraTransform.position, helperCameraTransform.forward);
        Vector3 hitPoint = ray.GetPoint(currentRaycast.distance);
        // Angle Calculation
        Vector3 helperCameraPosition = _helperCamera.transform.position;
        Vector3 directionA = Vector3.Normalize(helperCameraPosition - hitPoint);
        Vector3 directionB = Vector3.Normalize(helperCameraPosition - _lastRaycastHitPoint);
        _pointerEventData.delta = new Vector2(Vector3.Angle(directionA, directionB), 0);
        _lastRaycastHitPoint = hitPoint;

        m_RaycastResultCache.Clear();
        return _pointerEventData;
    }

    bool TriggerReleasedThisFrame() {
        return (_triggerPressedLastFrame && !_triggerPressed);
    }

    bool TriggerPressedThisFrame() {
        return (!_triggerPressedLastFrame && _triggerPressed);
    }

    // Copied from StandaloneInputModule
    private bool SendUpdateEventToSelectedObject() {
        if (eventSystem.currentSelectedGameObject == null)
            return false;

        BaseEventData data = GetBaseEventData();
        ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
        return data.used;
    }

    // Modified from StandaloneInputModule
    public override void ActivateModule() {
        base.ActivateModule();

        GameObject toSelect = eventSystem.currentSelectedGameObject;
        if (toSelect == null)
            toSelect = eventSystem.firstSelectedGameObject;

        eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());

        if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
            return;

        base.ActivateModule();
    }

    private static bool ShouldIgnoreEventsOnNoFocus() {
        switch (SystemInfo.operatingSystemFamily) {
            case OperatingSystemFamily.Windows:
            case OperatingSystemFamily.Linux:
            case OperatingSystemFamily.MacOSX:
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isRemoteConnected)
                    return false;
#endif
                return true;
            default:
                return false;
        }
    }

    // Copied from StandaloneInputModule
    public override void DeactivateModule() {
        base.DeactivateModule();
        ClearSelection();
    }

    private bool ShouldStartDrag(float threshold, bool useDragThreshold) {
        if (!useDragThreshold)
            return true;
        return _pressedDistance >= threshold;
    }

    private void ProcessVRDrag(PointerEventData eventData) {
        // If pointer is not moving or if a button is not pressed (or pressed control did not return drag handler), do nothing
        if (!eventData.IsPointerMoving() || eventData.pointerDrag == null)
            return;

        // We are eligible for drag. If drag did not start yet, add drag distance
        if (!eventData.dragging) {
            _pressedDistance += eventData.delta.x;

            if (ShouldStartDrag(eventSystem.pixelDragThreshold, eventData.useDragThreshold)) {
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                eventData.dragging = true;
            }
        }

        // Drag notification
        if (eventData.dragging) {
            // Before doing drag we should cancel any pointer down state
            // And clear selection!
            if (eventData.pointerPress != eventData.pointerDrag) {
                ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                eventData.eligibleForClick = false;
                eventData.pointerPress = null;
                eventData.rawPointerPress = null;
            }

            ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
        }
    }

    //private void SetupControllers([NotNull] SteamVR_PlayArea steamVRPlayArea) {
    //    if (steamVRPlayArea == null) throw new ArgumentNullException(nameof(steamVRPlayArea));
    //    foreach (SteamVR_Behaviour_Pose pose in
    //        steamVRPlayArea.GetComponentsInChildren<SteamVR_Behaviour_Pose>(true)) {
    //        if (pose.inputSource == SteamVR_Input_Sources.RightHand) {
    //            _controllerRight = pose.transform;
    //        }
    //        else if (pose.inputSource == SteamVR_Input_Sources.LeftHand) {
    //            _controllerLeft = pose.transform;
    //        }
    //    }
    //}

    #region PlayerInput
    private void SetupControllers([NotNull] PlayerRigBase playArea)
    {
        if (playArea == null) throw new ArgumentNullException(nameof(playArea));

        playArea.TryGetPlayerRigTransform(PlayerRigTransformOptions.LeftHand, out _controllerLeft);
        playArea.TryGetPlayerRigTransform(PlayerRigTransformOptions.RightHand, out _controllerRight);

        PlayerInputBase.GetInstance(PlayerHand.Left, out XRController_Left);
        PlayerInputBase.GetInstance(PlayerHand.Right, out XRController_Right);
    }
    #endregion
}