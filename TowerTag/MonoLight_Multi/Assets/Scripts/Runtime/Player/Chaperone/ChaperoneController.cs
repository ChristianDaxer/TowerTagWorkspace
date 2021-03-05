using TowerTagSOES;
using UnityEngine;

public class ChaperoneController : MonoBehaviour {
    [SerializeField] private GameObject _chaperoneQuadFront;
    [SerializeField] private GameObject _chaperoneQuadsSideRight;
    [SerializeField] private GameObject _chaperoneQuadsSideLeft;
    [SerializeField] private GameObject _lightWall;
    private bool _missingReferences;
    private Vector3 _startPositionQuadFront;
    private Vector3 _startPositionQuadSide;
    private Vector3 _startScaleQuadSide;
    private Vector3 _standAloneQuadFront;
    private Vector3 _standAloneQuadSidePos;
    private Vector3 _standAloneQuadSideScale;
    private bool _initialized;
    private Vector3 _lightWallStartPosition;
    private Vector3 _lightWallStartScale;
    private Vector3 _standAloneLightWallPosition;
    private Vector3 _standAloneLightWallScale;

    private void OnEnable() {
        // Early Returns

        if (!TowerTagSettings.Home) {
            gameObject.SetActive(false);
            return;
        }

#if !UNITY_EDITOR
        if (!SharedControllerType.VR)
        {
            gameObject.SetActive(false);
            return;
        }
#endif

        ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;

        if (!_initialized)
            Init();
    }

    private void OnDisable() {
        ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
    }

    private void Init() {
        if (_chaperoneQuadFront == null) {
            Debug.LogError("ChaperoneController: Missing room scale play space chaperone reference.");
            _missingReferences = true;
        }

        if (_chaperoneQuadsSideRight == null) {
            Debug.LogError("ChaperoneController: Missing standalone play space chaperone reference.");
            _missingReferences = true;
        }

        if (_chaperoneQuadsSideLeft == null) {
            Debug.LogError("ChaperoneController: Missing standalone play space chaperone reference.");
            _missingReferences = true;
        }

        _missingReferences = false;

        // Get Start values
        _startPositionQuadFront = _chaperoneQuadFront.transform.localPosition;
        _startScaleQuadSide = _chaperoneQuadsSideLeft.transform.localScale;
        _startPositionQuadSide = _chaperoneQuadsSideLeft.transform.localPosition;
        _lightWallStartPosition = _lightWall.transform.localPosition;
        _lightWallStartScale = _lightWall.transform.localScale;

        // Set standalone values
        _standAloneQuadFront = _startPositionQuadFront + new Vector3(0, 0, -1);
        _standAloneQuadSidePos = _startPositionQuadSide + new Vector3(0, 0, -0.5f);
        _standAloneQuadSideScale = _startScaleQuadSide + new Vector3(-1, 0, 0);
        _standAloneLightWallPosition = _lightWallStartPosition + new Vector3(0, 0, -0.5f);
        _standAloneLightWallScale = _lightWallStartScale + new Vector3(0, 0, -0.5f);

        UpdateChaperone(ConfigurationManager.Configuration.SmallPlayArea);
        _initialized = true;
    }

    private void OnConfigurationUpdated() {
        UpdateChaperone(ConfigurationManager.Configuration.SmallPlayArea);
    }

    private void UpdateChaperone(bool standAlonePlaySpace) {
        if (_missingReferences) return;

        if (standAlonePlaySpace) {
            _chaperoneQuadFront.transform.localPosition = _standAloneQuadFront;
            _chaperoneQuadsSideLeft.transform.localPosition = _standAloneQuadSidePos;
            _chaperoneQuadsSideRight.transform.localPosition = _standAloneQuadSidePos + new Vector3(2,0,0);
            _chaperoneQuadsSideLeft.transform.localScale = _standAloneQuadSideScale;
            _chaperoneQuadsSideRight.transform.localScale = _standAloneQuadSideScale;
            _lightWall.transform.localPosition = _standAloneLightWallPosition;
            _lightWall.transform.localScale = _standAloneLightWallScale;
        }
        else
            ResetChaperone();
    }

    private void ResetChaperone() {
        _chaperoneQuadFront.transform.localPosition = _startPositionQuadFront;
        _chaperoneQuadsSideLeft.transform.localPosition = _startPositionQuadSide;
        _chaperoneQuadsSideRight.transform.localPosition = _startPositionQuadSide + new Vector3(2,0,0);
        _chaperoneQuadsSideLeft.transform.localScale = _startScaleQuadSide;
        _chaperoneQuadsSideRight.transform.localScale = _startScaleQuadSide;
        _lightWall.transform.localPosition = _lightWallStartPosition;
        _lightWall.transform.localScale = _lightWallStartScale;
    }
}