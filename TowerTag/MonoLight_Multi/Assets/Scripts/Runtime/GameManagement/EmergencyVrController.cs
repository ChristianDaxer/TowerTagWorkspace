using AmplifyBloom;
using UnityEngine;
using UnityEngine.Serialization;

public class EmergencyVrController : MonoBehaviour {
    private Camera _hmdCamera;

    [FormerlySerializedAs("emergencyCanvas")] [SerializeField]
    private Canvas _emergencyCanvas;

    [Space, Header("Emergency Canvas Settings")]
    private readonly Vector3 _emergencyCanvasOffset = new Vector3(0, 0, 1);

    [FormerlySerializedAs("emergencyCanvasSmoothTime")] [SerializeField]
    private float _emergencyCanvasSmoothTime = 0.3f;


    [Space, Header("Objects to deactivate")] [SerializeField]
    private Chaperone _chaperone;

    [FormerlySerializedAs("bloom")] [SerializeField]
    private AmplifyBloomEffect _bloom;

    [FormerlySerializedAs("saturation")] [SerializeField]
    private Saturation _saturation;


    private bool _emergency;

    private Vector3 _emergencyCanvasVelocity;

    private LayerMask _defaultCullingMask;

    private void Start() {
        if (PlayerHeadBase.GetInstance(out var playerHeadBase)) {
            _hmdCamera = playerHeadBase.HeadCamera;
            _bloom = _hmdCamera.GetComponent<AmplifyBloomEffect>();
            _saturation = _hmdCamera.GetComponent<Saturation>();
        }


        _defaultCullingMask = _hmdCamera.cullingMask;
        SetEmergency(_emergency);
    }

    private void OnEnable() {
        GameManager.Instance.EmergencyReceived += TriggerEmergency;
    }

    private void OnDisable() {
        if (GameManager.Instance != null)
            GameManager.Instance.EmergencyReceived -= TriggerEmergency;
    }

    private void Update() {
        if (_emergency) {
            Vector3 targetPosition = _hmdCamera.transform.TransformPoint(_emergencyCanvasOffset);
            Transform emergencyCanvasTransform;
            (emergencyCanvasTransform = _emergencyCanvas.transform).position = Vector3.SmoothDamp(
                _emergencyCanvas.transform.position,
                targetPosition,
                ref _emergencyCanvasVelocity,
                _emergencyCanvasSmoothTime);
            emergencyCanvasTransform.rotation = _hmdCamera.transform.rotation;
        }
    }

    private void TriggerEmergency() {
        Debug.LogWarning("Received Emergency Event!");
        SetEmergency(true);
    }

    /// <summary>
    /// Toggle emergency
    /// </summary>
    /// <param name="newEmergencyState"></param>
    private void SetEmergency(bool newEmergencyState) {
        _emergency = newEmergencyState;
        _emergencyCanvas.gameObject.SetActive(_emergency);

        if (_hmdCamera != null) {
            if (_emergency) {
                _hmdCamera.cullingMask = 1 << _emergencyCanvas.gameObject.layer;
                _hmdCamera.clearFlags = CameraClearFlags.SolidColor;
                _hmdCamera.backgroundColor = Color.black;
            }
            else {
                _hmdCamera.cullingMask = _defaultCullingMask;
                _hmdCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
        else {
            Debug.LogError("HMD Camera object in EmergencyVrController missing!");
        }

        if (_chaperone == null)
        {
            var newChaperone = FindObjectOfType(typeof(Chaperone));
            _chaperone = (Chaperone) (newChaperone != null ? newChaperone : null);
        }

        if (_chaperone != null) {
            if (_emergency) {
                // Emergency occured
                _chaperone.SetActive(false);
                _chaperone.gameObject.SetActive(false);
            }
            else {
                // Reset Chaperone to active
                _chaperone.gameObject.SetActive(true);
                _chaperone.SetActive(true);
            }
        }

        if (_bloom != null) {
            _bloom.enabled = !_emergency;
        }
        else {
            Debug.LogError("Bloom script in EmergencyVrController missing!");
        }

        if (_saturation && _emergency) {
            _saturation.enabled = false;
        }
    }
}