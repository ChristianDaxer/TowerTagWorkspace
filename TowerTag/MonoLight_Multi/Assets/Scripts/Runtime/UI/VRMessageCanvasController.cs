using TowerTagSOES;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

[RequireComponent(typeof(OverlayCanvasModel), typeof(BadaboomHyperactionPointerNeeded))]
public class VRMessageCanvasController : MonoBehaviour {
    public delegate void VRMessageCanvasActive(object sender, bool status);

    public event VRMessageCanvasActive VRMessageCanvasActivated;

#if !UNITY_ANDROID
    private SteamVR_PlayArea _playArea;
#endif
    [SerializeField] private Vector3 _offset;
    [SerializeField] private BadaboomHyperactionPointer _pointerPrefab;
    private Transform _transform;
    private BadaboomHyperactionPointer _pointer;
    private BadaboomHyperactionPointerNeeded _badaboomHyperactionPointerNeeded;
    private OverlayCanvasModel _overlayCanvasModel;
    private Camera _cam;
    [SerializeField, Tooltip("only to debug")] private bool _canvasActive;

    public bool CanvasActive {
        get => _canvasActive;
        private set => _canvasActive = value;
    }

    private void Awake() {
        if (!TowerTagSettings.Home || !SharedControllerType.IsPlayer)
            Destroy(gameObject);
    }

    private void Start() {
        _transform = transform;
        _badaboomHyperactionPointerNeeded = GetComponent<BadaboomHyperactionPointerNeeded>();
        _overlayCanvasModel = GetComponent<OverlayCanvasModel>();
        var overlayCanvasModel = GetComponent<OverlayCanvasModel>();
        overlayCanvasModel.OnOpen += OnCanvasOpened;
        overlayCanvasModel.OnClose += OnCanvasClosed;
    }

    private void OnCanvasOpened() {
        if (!BadaboomHyperactionPointer.GetInstance(out _pointer)) {
            _pointer = InstantiateWrapper.InstantiateWithMessage(_pointerPrefab);
        }
        /*if (_overlayCanvasModel.Canvas.isActiveAndEnabled)
            CanvasActive = true;*/

        VRMessageCanvasActivated?.Invoke(this, true);
    }

    private void OnCanvasClosed() {
        /*if (!_overlayCanvasModel.Canvas.isActiveAndEnabled)
            CanvasActive = false;*/
        VRMessageCanvasActivated?.Invoke(this, false);
    }

    private void Update() {
        CanvasActive = _overlayCanvasModel.Canvas.isActiveAndEnabled;
        if (!CanvasActive) return;

        Pillar tower = null;
        if (TTSceneManager.Instance.IsInConnectScene)
        {
            if (!OffboardingPillarInstance.GetInstance(out var offboardingPillarInstance))
                return;
            tower = offboardingPillarInstance.PillarInstance;
        }
        else
        {
            TowerTag.IPlayer player = PlayerManager.Instance.GetOwnPlayer();
            if (player == null)
                Debug.LogError("There is no local player representing self in the game.");
            else tower = player.CurrentPillar;
        }

        /*
        if (MySceneManager.Instance.IsInConnectScene)
        {
            PrefabFactoryLobby prefabFactoryLobby = FindObjectOfType<PrefabFactoryLobby>();
            if (prefabFactoryLobby == null)
                Debug.LogErrorFormat("No {0} available in the scene.", typeof(PrefabFactoryLobby).FullName);
            else tower = prefabFactoryLobby.GetComponentInChildren<Pillar>();
        }
        else
        {
            TowerTag.IPlayer player = PlayerManager.Instance.GetOwnPlayer();
            if (player == null)
                Debug.LogError("There is no local player representing self in the game.");
            else tower = player.CurrentPillar;
        }
        */

        if (tower == null) return;
        if (_cam == null)
            _cam = Camera.main;
        var cameraRotation = _cam.transform.rotation;
        Quaternion rotation = Quaternion.Euler(0, Mathf.RoundToInt(cameraRotation.eulerAngles.y / 90) * 90, 0);
        _transform.position =
            tower.transform.position + rotation * _offset; //tower.transform.InverseTransformVector(_offset);
        _transform.rotation = rotation;
        TogglePointerNeededTag();
    }

    private void TogglePointerNeededTag() {
        _badaboomHyperactionPointerNeeded.enabled = _overlayCanvasModel.Canvas.isActiveAndEnabled;
    }
}