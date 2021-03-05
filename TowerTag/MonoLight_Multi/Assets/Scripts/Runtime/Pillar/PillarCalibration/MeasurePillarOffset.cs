using UnityEngine;
//using Valve.VR;

// Use:
//      0) start game in measure scene
//      1) set 4 points (clockwise or counterclockwise) with steamVR controller hairTrigger
//      2) fine tune position & Rotation with keyboard
//      3) save values to configFile
//      4) start real game and test

// TODO:
// cleanup
// rewrite config: angle instead of quaternion                         *
// test with rotated cubes/player/Pillar
// add Offset to Teleport & test                                        ~
// add keyboard fine tuning or per touch pad (change mode per grab
//      - setPoints(Trigger & Pad clicked)/
//      - Move (with touch pad & Trigger)/
//      - Rotate(with touch pad & Trigger) (Label over touch pad))
// add help
// inputSwitch like in input controller
// visual feedback when saved!!!!
// reset Pillar or show originalPosition
// switch between sceneCam & VRCam


//  Steps:
//      - 1 mm move pro taste
//      - 5 mm mit shift
//      - 0,5° angle -> 0.1 °
//      - 2,5° angle -> 0.5 °

public class MeasurePillarOffset : MonoBehaviour {
    // default controller
//    [SerializeField] private SteamVR_TrackedController _activeController;

    // controllers to switch (1 & 2)
    [SerializeField] private PlayerInputBase _primaryController;
    [SerializeField] private PlayerInputBase _secondaryController;


    PlayerInput _rightXRController;
    PlayerInput _leftXRController;

    // transform parented to controller at which the point positions are measured
    [SerializeField] private Transform _measureTransform;

    // handles to give visual feedback of measured points to the user
    [SerializeField] private GameObject[] _handles;

    // measured points (size has to be 4)
    private Vector3[] _points;
    private int _currentPointIndex;

    // object we want to move/rotate to see if measurement is correct
    [SerializeField] private Transform _targetObject;

    // cache original values of targetObject
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    // calculated offsets for position & rotation
    private Vector3 _positionOffset;
    private float _offsetRotationAngle;
    private bool _moveInWorldSpace;

    // Use this for initialization
    private void Start() {
        // Values
        _points = new Vector3[4];

        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        _originalPosition = _targetObject.position;
        _originalRotation = _targetObject.rotation;

        _rightXRController.OnTriggerDown += TriggerClicked;
        _leftXRController.OnTriggerDown += TriggerClicked;

        _rightXRController.OnMove += PadClicked;
        _leftXRController.OnMove += PadClicked;

        _primaryController = _rightXRController;
        _secondaryController = _leftXRController;

        _measureTransform = GetMeasurePoint();

        _rightXRController.calibrationMode = true;
        _leftXRController.calibrationMode = true;

        VRController.ActivateOpenVR();


        // Reset Visuals
        Reset();
    }

    private void OnDestroy() {

        _rightXRController.calibrationMode = false;
        _leftXRController.calibrationMode = false;

        _rightXRController.OnTriggerDown -= TriggerClicked;
        _leftXRController.OnTriggerDown -= TriggerClicked;

        _rightXRController.OnMove -= PadClicked;
        _leftXRController.OnMove -= PadClicked;
    }

    // SteamVR Controller callbacks
    private void TriggerClicked(PlayerInputBase fromSource, bool newState) {
       
        if (_primaryController != fromSource) return;
        if(newState) AddPointToMeasure();
    }

    private void PadClicked(PlayerInputBase fromSource, Vector2 newState) {
     //   Debug.Log("PadClicked " + newState);
        if (_primaryController != fromSource) return;
        if (newState != Vector2.zero) Reset();
    }

    // switch from right to left controller & vice versa
    private void SwitchControllerIDs() {
        PlayerInputBase temp = _primaryController;
        _primaryController = _secondaryController;
        _secondaryController = temp;
        _measureTransform = GetMeasurePoint();
    }

    private Transform GetMeasurePoint()
    {
        Transform[] trans = _primaryController.GetComponentsInChildren<Transform>(true);
        Transform result = null;
        for (int i = 0; i < trans.Length; i++)
        {
            if (trans[i].name.Equals("MeasurePoint"))
                result = trans[i];
        }
        return result;
    }

    // add new measure point
    private void AddPointToMeasure() {
        Vector3 position = _measureTransform.position;
        _points[_currentPointIndex] = position;
        _handles[_currentPointIndex].transform.position = position;
        ShowHandle(_currentPointIndex, true);
        _currentPointIndex++;

        if (_currentPointIndex == 4) {
            _currentPointIndex = 0;
            CalculateOffset();
        }
    }

    // calculate rotation & position offset of measured points
    // (points form a plane, it's rotation & position rel. to the default plane (axis oriented and centered at origin (0,0,0) is give the offsets))
    private void CalculateOffset() {
        Vector3 center = CalibrationToolAlgorithms.GetCenter(_points);
        _positionOffset = _originalPosition - center;
        _offsetRotationAngle = CalibrationToolAlgorithms.GetRotationOffsetAroundYAxis(_points, center);

        // Visuals
        _handles[4].transform.position = _originalPosition - _positionOffset;

        foreach (GameObject handle in _handles) {
            handle.transform.rotation = Quaternion.AngleAxis(_offsetRotationAngle, Vector3.up) * _originalRotation *
                                        handle.transform.rotation;
        }

        ShowHandle(4, true);
        UpdateTargetObject();

      //  PrintDebugInfo(center, _offsetRotationAngle);
    }

    private void PrintDebugInfo(Vector3 center, float angle) {
        var i = 0;
        foreach (Vector3 p in _points) {
            Debug.Log("point " + i++ + ": " + p.ToString("F4"));
        }

        Debug.Log("Center: " + center.ToString("F4") + " angle: " + angle.ToString("F4"));
        Debug.Log("_positionOffset: " + _positionOffset.ToString("F4") + " _offsetRotationAngle: " +
                  _offsetRotationAngle.ToString("F4"));
    }

    // move targetObject by offset from its original position
    private void UpdateTargetObject() {
        _targetObject.position = _originalPosition - _positionOffset;
        _targetObject.rotation = Quaternion.AngleAxis(_offsetRotationAngle, Vector3.up) * _originalRotation;
    }

    // show/hide handles
    private void ShowHandle(int index, bool show) {
        _handles[index].SetActive(show);
    }

    private void ShowHandles(bool show) {
        foreach (GameObject handle in _handles) {
            handle.SetActive(show);
        }
    }

//    private void OnGUI() {
//        if (GUILayout.Button("Switch to " + ((_moveInWorldSpace) ? "local" : "global") + " Movement")) {
//            _moveInWorldSpace = !_moveInWorldSpace;
//        }
//
//        if (GUILayout.Button(" Reset (Click Pad)")) {
//            Reset();
//        }
//
//        if (GUILayout.Button(" ** Save To ConfigFile ** (Click MenuButton)")) {
//            SaveToDisk();
//        }
//
//        if (GUILayout.Button("Quit Offset Tool")) {
//            QuitApplication();
//        }
//
//        if (_showSaved) {
//            GUILayout.TextField("Saved to config file!");
//        }
//    }

    private void Update() {
        //UpdateKeyInput_Smooth();

        if (Input.GetKeyDown(KeyCode.X) || (  !_primaryController.isConnected )) {
            SwitchControllerIDs();
        }

        UpdateKeyInput_Steps();
        UpdateTargetObject();
    }

    // stepwise Pillar movement with Key input
    private void UpdateKeyInput_Steps() {
        // Position
        if (Input.GetKeyUp(KeyCode.T)) {
            _moveInWorldSpace = !_moveInWorldSpace;
        }

        float stepWidth = Input.GetKey(KeyCode.LeftShift) ? 0.005f : 0.001f;
        float stepX = 0, stepY = 0;

        // left
        if (Input.GetKeyDown(KeyCode.A)) {
            stepX = stepWidth;
        }
        // right
        else if (Input.GetKeyDown(KeyCode.D)) {
            stepX = -stepWidth;
        }

        // forward
        if (Input.GetKeyDown(KeyCode.W)) {
            stepY = -stepWidth;
        }
        // backward
        else if (Input.GetKeyDown(KeyCode.S)) {
            stepY += stepWidth;
        }

        if (_moveInWorldSpace) {
            _positionOffset.x += stepX;
            _positionOffset.z += stepY;
        }
        else {
            _positionOffset += _targetObject.right * stepX + _targetObject.forward * stepY;
        }

        _positionOffset.y = 0;

        // Rotation
        float rotationStep = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 0.1f;
        // left
        if (Input.GetKeyDown(KeyCode.Q)) {
            _offsetRotationAngle += rotationStep;
        }
        // right
        else if (Input.GetKeyDown(KeyCode.E)) {
            _offsetRotationAngle -= rotationStep;
        }
    }

    public void SaveToDisk() {
        if (string.IsNullOrEmpty(ConfigurationManager.Path))
            ConfigurationManager.Path = Application.persistentDataPath + "/";
//        ConfigurationManager.LoadConfigFromFile();

        ConfigurationManager.Configuration.PillarPositionOffset =
            _positionOffset; //chaperone.InverseTransformDirection( _positionOffset);
        ConfigurationManager.Configuration.PillarRotationOffsetAngle = _offsetRotationAngle;

        ConfigurationManager.WriteConfigToFile();
    }

    private void Reset() {
        _positionOffset = Vector3.zero;
        _offsetRotationAngle = 0f;

        _currentPointIndex = 0;
        ShowHandles(false);
    }
}