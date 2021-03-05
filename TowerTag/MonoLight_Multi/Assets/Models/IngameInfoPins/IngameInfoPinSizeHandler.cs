using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Resizes the pin by distance, get enabled/disabled by animator
/// </summary>
public class IngameInfoPinSizeHandler : MonoBehaviour {
    [SerializeField][FormerlySerializedAs("size")] private float _size = 4f;
    private readonly float lineSize = 20f;

    [SerializeField][FormerlySerializedAs("pointB")] private GameObject _pointB;
    [SerializeField][FormerlySerializedAs("pinPointGraphic")]private GameObject _pinPointGraphic;
    [SerializeField][FormerlySerializedAs("lineRendererAB")] private IngameInfoPinLineRendererHandler _lineRendererAb;
    [SerializeField][FormerlySerializedAs("lineRendererBC")] private IngameInfoPinLineRendererHandler _lineRendererBc;
    private int _maxScaleDistance = 100;
    private Vector3 _pointBInitialSize;
    private Vector3 _pinPointGraphicInitialSize;
    private float _lineRendererAbInitialSize;
    private float _lineRendererBcInitialSize;

    private float _distancePinToCamera;
    Camera _mainCamera;
    private float _cameraRelOverallSize;
    private Vector3 _initScale;

    // Start is called before the first frame update
    void Start() {

        if (PlayerHeadBase.GetInstance(out var playerHeadBase))
            _mainCamera = playerHeadBase.HeadCamera;

        _initScale = transform.localScale;
        _pointBInitialSize = _pointB.transform.localScale;
        _pinPointGraphicInitialSize = _pinPointGraphic.transform.localScale;
        _lineRendererAbInitialSize = _lineRendererAb.lineWidthFactor;
        _lineRendererBcInitialSize = _lineRendererBc.lineWidthFactor;
    }

    private void Update() {
        _distancePinToCamera = Vector3.Distance(_mainCamera.transform.position, transform.position);
        float cameraRelLineSize = (lineSize / _distancePinToCamera) * _size;

        // change size
        if(_pointB != null) _pointB.transform.localScale = _pointBInitialSize * _size;
        if(_pinPointGraphic != null) _pinPointGraphic.transform.localScale = _pinPointGraphicInitialSize * _size;
        if(_lineRendererAb != null) _lineRendererAb.lineWidthFactor = _lineRendererAbInitialSize * cameraRelLineSize;
        if(_lineRendererBc != null) _lineRendererBc.lineWidthFactor = _lineRendererBcInitialSize * cameraRelLineSize;
        transform.localScale = _initScale * (1 + _distancePinToCamera / _maxScaleDistance);
    }
}
