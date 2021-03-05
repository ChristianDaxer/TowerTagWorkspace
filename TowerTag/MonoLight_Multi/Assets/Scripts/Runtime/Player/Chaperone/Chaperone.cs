using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class Chaperone : MonoBehaviour {
    [SerializeField] private Vector2 _chaperoneExtends = new Vector2(1, 1);
    [SerializeField] private float _borderWidth = 0.1f;
    [SerializeField] private float _chaperoneBlurOffset = 0.1f;

    [SerializeField] private float _minHeight = 0.15f, _maxHeight = 0.7f;
    [SerializeField] private Material _material;

    [SerializeField] private GameObject _visualsParent;
    [SerializeField] private Transform[] _blurColliderTransforms;
    [SerializeField] private InputControllerVR _inputControllerVR;
    private Transform _controller;

    // cache values
    private Vector3 _lastHMDPosition = Vector3.zero;

    // bounds
    private Vector3 _boundsCenter = Vector3.zero;
    private float _boundsMinX, _boundsMaxX, _boundsMinZ, _boundsMaxZ;

    private bool _isActive;

    // detect if user is outside of chaperone bounds
    private static readonly int _deltaHeight = Shader.PropertyToID("_DeltaHeight");
    private static readonly int _controllerWorldPosition = Shader.PropertyToID("_ControllerWorldPosition");
    private static readonly int _hmdWorldPosition = Shader.PropertyToID("_HMDWorldPosition");
    private IPlayer _player;

    private PlayerHeadBase playerHeadBase;

    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
        if (!SharedControllerType.IsPlayer || _player == null) {
            enabled = false;
        }
        
        if (_controller == null)
            GetController();
    }

    private void GetController ()

    {
        if (PlayerInputBase.GetInstance(PlayerHand.Right, out var playerInputBase))
        {
            _controller = playerInputBase.transform;
        }
    }

    private void OnEnable() {
        GetController();
    }

    private void OnDisable() {
        if (_player != null) _player.IsOutOfChaperone = false;
        SetControllerAlpha(0);
        SetControllerPosition(Vector3.zero);
        SetHMDPosition(Vector3.zero);
    }

    private void Update() {
        if (_isActive)
            UpdateController();
    }

    private void Init() {
        _visualsParent.transform.localScale = transform.worldToLocalMatrix * new Vector4(_chaperoneExtends.x, 1, _chaperoneExtends.y, 0);
        SetChaperoneBlurOffset();

        // init Shader Values
        Vector3 position = transform.position;
        SetControllerPosition(position);
        SetHMDPosition(position);
        SetControllerAlpha(_minHeight);
    }

    private void UpdateController() {

        // tmp
        float delta = 0;

        // update bounds
        _boundsCenter = transform.position;
        _boundsMinX = _boundsCenter.x - _chaperoneExtends.x;
        _boundsMaxX = _boundsCenter.x + _chaperoneExtends.x;
        _boundsMinZ = _boundsCenter.z - _chaperoneExtends.y;
        _boundsMaxZ = _boundsCenter.z + _chaperoneExtends.y;

        // controller
        if (playerHeadBase == null)
        {
            if (!PlayerHeadBase.GetInstance(out playerHeadBase))
                return;
        }

        Vector3 currentHMDPosition = playerHeadBase.transform.position;
        // if (_controller != null && _controller.gameObject.activeInHierarchy) {
        //     Vector3 controllerPosition = _controller.position;
        //     delta = CheckForChaperoneIntersection(controllerPosition);
        //     Vector3 position = controllerPosition;
        //
        //     if (delta >= 1) {
        //         position = ProjectPositionOnWalls(position, position - currentHMDPosition);
        //     }
        //
        //     if (_lastControllerPosition != position) {
        //         _lastControllerPosition = position;
        //         SetControllerPosition(position);
        //     }
        // }

        // HMD
        float hmdDelta = CheckForChaperoneIntersection(currentHMDPosition);
        PlayersHeadLeftChaperoneBounds(hmdDelta >= 1);

        delta = Mathf.Max(hmdDelta, delta);

        if (_lastHMDPosition != currentHMDPosition) {
            _lastHMDPosition = currentHMDPosition;
            SetHMDPosition(currentHMDPosition);
        }

        // wallHeight
        SetControllerAlpha(delta);
    }

    private void PlayersHeadLeftChaperoneBounds(bool playerIsOutsideOfBounds) {
        if (_player != null) _player.IsOutOfChaperone = playerIsOutsideOfBounds;
    }

    /*Function to show head in chaperone (removed currently)
    private Vector3 ProjectPositionOnWalls(Vector3 position, Vector3 direction) {
        float tMax = float.MaxValue;

        float t1 = (_boundsMinX - position.x) / direction.x;
        float t2 = (_boundsMaxX - position.x) / direction.x;
        tMax = Mathf.Min(tMax, Mathf.Max(t1, t2));

        t1 = (_boundsMinZ - position.z) / direction.z;
        t2 = (_boundsMaxZ - position.z) / direction.z;
        tMax = Mathf.Min(tMax, Mathf.Max(t1, t2));
        return position + tMax * direction;
    }
    */


    private float CheckForChaperoneIntersection(Vector3 position) {
        Vector2 delta = Vector2.zero;

        if (position.x <= _boundsMinX + _borderWidth) {
            delta.x = Mathf.InverseLerp(_boundsMinX + _borderWidth, _boundsMinX, position.x);
        } else if (position.x >= _boundsMaxX - _borderWidth) {
            delta.x = Mathf.InverseLerp(_boundsMaxX - _borderWidth, _boundsMaxX, position.x);
        }

        if (position.z <= _boundsMinZ + _borderWidth) {
            delta.y = Mathf.InverseLerp(_boundsMinZ + _borderWidth, _boundsMinZ, position.z);
        } else if (position.z >= _boundsMaxZ - _borderWidth) {
            delta.y = Mathf.InverseLerp(_boundsMaxZ - _borderWidth, _boundsMaxZ, position.z);
        }
        return Mathf.Max(delta.x, delta.y);
    }


    private void SetControllerAlpha(float delta) {
        _material.SetFloat(_deltaHeight, Mathf.Lerp(_minHeight, _maxHeight, delta));
    }

    private void SetControllerPosition(Vector3 position) {
        _material.SetVector(_controllerWorldPosition, new Vector4(position.x, position.y, position.z));
    }

    private void SetHMDPosition(Vector3 position) {
        _material.SetVector(_hmdWorldPosition, new Vector4(position.x, position.y, position.z));
    }

    private void OnApplicationQuit() {
        _isActive = false;
        SetHMDPosition(Vector3.zero);
        SetControllerPosition(Vector3.zero);
        SetControllerAlpha(0);
    }

    private void SetChaperoneBlurOffset() {
        foreach (Transform t in _blurColliderTransforms) {
            if (t == null)
                continue;

            var c = t.GetComponent<BoxCollider>();

            if (c == null)
                continue;

            Vector3 center = c.center;
            center.z = c.size.z * 0.5f + _chaperoneBlurOffset / t.lossyScale.z;
            c.center = center;
        }
    }

    public void SetActive(bool activate) {
        _isActive = activate;

        if (_visualsParent != null)
            _visualsParent.SetActive(_isActive);

        if (_isActive) {
            Init();
        }
    }
}
