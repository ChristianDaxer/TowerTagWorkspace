using System.Collections.Generic;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
//using Valve.VR;
using VLB;

public class BadaboomHyperactionPointer : TTSingleton<BadaboomHyperactionPointer>
{
    public delegate void BadaboomHyperactionPointerAction(BadaboomHyperactionPointer sender, bool active);

    public event BadaboomHyperactionPointerAction BHAPState;

    [SerializeField, Tooltip("Used to detect if the pointer has changed in the input module.")]
    private Hand? _temp;

    [SerializeField, Tooltip("Used to switch active Hand")]
    private bool _allowAutoSwitchHand;

    [SerializeField, Tooltip("Laser pointer visualization")]
    private GameObject _laserPointer;

    [SerializeField, Tooltip("Sphere that visualizes the hit point of the pointer")]
    private Transform _hitPointSphere;

    [SerializeField] private LayerMask _layerMask;

    [Header("Visuals")] [SerializeField] private Team _colorizingTeamColor;
    [SerializeField] private MeshRenderer _sphere;
    [SerializeField] private VolumetricLightBeam _lightBeam;
    [SerializeField] private VolumetricLightBeam _lightBeam2;

    private BadaboomHyperactionInputModule _inputModule;
    private Transform _transform;
    private IPlayer _localPlayer;
    private Team _colorizedAs;

    [Header("Debug")]
    [SerializeField] private bool _showAvailableInterfaces;
    private static readonly List<int> _availableInterfaces = new List<int>();

    private RaycastHit[] hits = new RaycastHit[1];

    private int rayHits;
    public static void RegisterAvailableInterface(BadaboomHyperactionPointerNeeded sender) {
        _availableInterfaces.Add(sender.GetInstanceID());
    }

    public static void UnregisterAvailableInterface(BadaboomHyperactionPointerNeeded sender) {
        _availableInterfaces.Remove(sender.GetInstanceID());
    }

    protected new void Awake()
    {
        base.Awake();

        if (!SharedControllerType.VR && !SharedControllerType.NormalFPS) {
            // Destroy if the controller type are eg operator
            Debug.LogError("DESTROY");
            Destroy(gameObject);
            return;
        }

        _localPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (_localPlayer == null)
            return;

        _localPlayer.InitBadaboomHyperactionPointer(this);

        _laserPointer.SetActive(false);
    }

    private void Start()
    {
        _transform = transform;
        _inputModule = FindObjectOfType<BadaboomHyperactionInputModule>();
        if (null == _inputModule)
            return;

        _inputModule.SetController(this);
    }

    private void OnValidate()
    {
        if (_colorizedAs != _colorizingTeamColor)
            ColorizeBeam();
    }

    private void Update()
    {
        if (_inputModule == null) {
            Debug.LogErrorFormat("Null reference to {0}. (FIX ME)", nameof(BadaboomHyperactionInputModule));
            enabled = false;
            return;
        }

        if (_inputModule.XRController_Left == null && _inputModule.XRController_Right == null) { 
            Debug.LogErrorFormat("No valid references in: {0} to left or right controllers. (FIX ME)", nameof(BadaboomHyperactionInputModule));
            enabled = false;
            return;
        }

        if (!CheckPossiblePointerAction()) {
            Debug.LogError("DESTROY");
            Destroy(gameObject);
            return;
        }


        rayHits = Physics.RaycastNonAlloc(transform.position, transform.TransformDirection(Vector3.forward), hits, Mathf.Infinity, _layerMask);

        if (rayHits > 0) {
            _hitPointSphere.position = hits[0].point;
            if (!_laserPointer.activeSelf)
                TogglePointer();
        }
        else
            if (_laserPointer.activeSelf)
                TogglePointer();

        if (_temp != _inputModule._hand && _inputModule != null)
            if (SetPointer(_inputModule._hand))
                _temp = _inputModule._hand;

        if (_allowAutoSwitchHand && SharedControllerType.VR) {
            if (_inputModule.XRController_Right != null && _inputModule.XRController_Right.TriggerDown && _inputModule._hand != Hand.RightHand)
                SetPointer(Hand.RightHand);

            else if (_inputModule.XRController_Left != null && _inputModule.XRController_Left.TriggerDown && _inputModule._hand != Hand.LeftHand)
                SetPointer(Hand.LeftHand);

        }
    }

    private void OnDestroy()
    {
        if (null != _inputModule)
            _inputModule.RemoveController(this);

        BHAPState?.Invoke(this, false);
    }

    private void OnGUI()
    {
        if (_showAvailableInterfaces) GUI.Label(new Rect(0, 0, 200, 200), $"Available interfaces tracked by BadaboomHyperInteractionPointer: {string.Join(", ", _availableInterfaces)}");
    }

    private void ColorizeBeam()
    {
        _sphere.sharedMaterial.color = _colorizingTeamColor.Colors.Main;
        _lightBeam.color = _colorizingTeamColor.Colors.Effect;
        _lightBeam2.color = _colorizingTeamColor.Colors.Main;
        _colorizedAs = _colorizingTeamColor;
    }

    private bool SetPointer(Hand targetHand)
    {
        if (null != _inputModule && _transform != null)
        {
            _inputModule._hand = targetHand;
            var pos = _transform.localPosition;
            if (_localPlayer != null && _localPlayer.GunController != null && _localPlayer.GunController.RailGun != null)
                _transform.SetParent(_localPlayer.GunController.RailGun.transform);
            else
            {
                GunInTowerDetection gun = FindObjectOfType<GunInTowerDetection>();
                if (gun == null) return false;
                var playerNameOnGun = FindObjectOfType<PlayerNameOnGun>().transform;
                if(playerNameOnGun != null)
                    _transform.SetParent(playerNameOnGun);
            }
            ResetTransform(_transform, pos);
            return true;
        }

        return false;
    }

    private void ResetTransform(Transform trans, Vector3 pos)
    {
        trans.localPosition = pos;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }

    private void TogglePointer()
    {
        _laserPointer.SetActive(!_laserPointer.activeSelf);
        BHAPState?.Invoke(this, _laserPointer.activeSelf);
    }

    private static bool CheckPossiblePointerAction()
    {
        return _availableInterfaces.Count > 0;
    }

    protected override void Init() {}
}

public enum Hand
{
    RightHand,
    LeftHand
}