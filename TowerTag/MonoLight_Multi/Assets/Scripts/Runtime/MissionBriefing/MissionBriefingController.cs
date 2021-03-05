using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hub;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class MissionBriefingController : MonoBehaviour {
    [SerializeField, Tooltip("Operator cinemachine virtual camera")]
    private GameObject _vCam;

    [Header("Preparation Settings")] [SerializeField, Tooltip("Time it takes to change settings for mission briefing")]
    private float _fadeDuration;

    [SerializeField, Tooltip("Fog start distance for the Mission Briefing")]
    private float _fogStartDistance = 3;

    [SerializeField, Tooltip("Fog end distance for the Mission Briefing")]
    private float _fogEndDistance = 3.5f;

    [SerializeField, Tooltip("Distance from the mission briefing to the player")]
    private float _missionBriefingDistance = 3;

    [SerializeField, Tooltip("Y position of the Mission Briefing object")]
    private float _missionBriefingY = 1;

    [Header("Mission Briefing Settings")] [SerializeField, Tooltip("The Animator for the Mission Briefing")]
    private Animator _missionBriefingAnimator;

    [SerializeField, Tooltip("The script for the animation events")]
    private MissionBriefingAnimationEventHandler _animationEventHandler;

    [SerializeField, Tooltip("The currently selected match description")]
    private MatchDescription _currentMatchDescription;

    [SerializeField, Tooltip("Text field for the name of the map")]
    private Text _mapText;

    [SerializeField, Tooltip("Text field for the name of the map")]
    private Text _modeText;

    [SerializeField, Tooltip("Text field for the mission text")]
    private Text _missionText;

    [SerializeField, Tooltip("Parent object for the stereoscopic screenshot objects")]
    private GameObject _stereoScreenShotParent;

    [Header("Toggle Objects")] [SerializeField, Tooltip("All hub lanes")]
    private HubLaneControllerBase[] _hubLanes;

    [SerializeField, Tooltip("Other GameObjects that should be disabled while Mission Briefing is running")]
    private GameObject[] _otherGameObjects;

    private Coroutine _mbCoroutine;
    private GameObject _matchDependentMissionBriefing;

    private MissionBriefingAnnouncerSoundHandler _announcer;

    private readonly Dictionary<GameMode, string> _missionDescriptions = new Dictionary<GameMode, string> {
        {GameMode.Elimination, "TAKE OUT THE ENEMY TEAM TO SCORE"},
        {GameMode.GoalTower, "CAPTURE THE ENEMY GOAL TOWER TO SCORE"},
        {GameMode.DeathMatch, "TAKE OUT AS MANY ENEMY PLAYERS AS POSSIBLE"}
    };

    private IPlayer _ownPlayer;
    private Material _defaultSkyboxMaterial;
    private Color _hubSceneBackgroundColor;
    private float _startFogDistance;
    private float _endFogDistance;
    private readonly List<Renderer> _renderers = new List<Renderer>();
    private readonly List<Canvas> _canvases = new List<Canvas>();
    private readonly List<TextMeshProUGUI> _textMeshPro = new List<TextMeshProUGUI>();
    private bool _isRunning;
    private (Pillar pillar, bool wasGlass) _missionBriefingPillar;

    private static readonly int _color = Shader.PropertyToID("_Tint");
    private static readonly int _startTriggerHash = Animator.StringToHash("Start");
    private static readonly int _abort = Animator.StringToHash("Abort");

    public MatchDescription CurrentMatchDescription {
        get => _currentMatchDescription;
        private set => _currentMatchDescription = value;
    }

    private void Awake() {
        _animationEventHandler.MissionBriefingController = this;
        _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        CacheStartValues();

        if (!SharedControllerType.IsAdmin) {
            if (_ownPlayer == null || _ownPlayer.GameObject == null) {
                Debug.LogWarning("No local player found! Can't place Mission Briefing object");
                enabled = false;
            }
        }
    }

    private void CacheStartValues() {
        _startFogDistance = RenderSettings.fogStartDistance;
        _endFogDistance = RenderSettings.fogEndDistance;
        _defaultSkyboxMaterial = RenderSettings.skybox;
        _hubSceneBackgroundColor = _defaultSkyboxMaterial.GetColor(_color);
    }

    private void OnEnable() {
        GameManager.Instance.MissionBriefingStarted += InitializeMissionBriefing;
        GameManager.Instance.MatchConfigurationStarted += AbortMissionBriefing;
    }

    private void OnDisable() {
        GameManager.Instance.MissionBriefingStarted -= InitializeMissionBriefing;
        GameManager.Instance.MatchConfigurationStarted -= AbortMissionBriefing;
    }

    public void InitializeMissionBriefing(MatchDescription matchDescription, GameMode gameMode) {
        if (_mbCoroutine != null) {
            Debug.LogWarning("Trying to start a second mission briefing! Abort!");
            return;
        }

        _mbCoroutine = StartCoroutine(PrepareMissionBriefing(matchDescription, gameMode));
    }

    private void Update() {
        if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator && _ownPlayer.GameObject != null) {
            SetMissionBriefingPosition(_ownPlayer.GameObject.transform);
        }
    }

    private void SetMissionBriefingPosition(Transform viewerTransform) {
        Vector3 forward = viewerTransform.forward;
        Vector3 position = viewerTransform.position + forward.normalized * _missionBriefingDistance;
        if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator)
            position = position + Vector3.up * _missionBriefingY;
        transform.position = position;
        transform.TransformDirection(forward.normalized);
    }

    private IEnumerator PrepareMissionBriefing(MatchDescription matchDescription, GameMode gameMode) {
        float time = 0;
        Material skybox = new Material(RenderSettings.skybox);
        Color defaultFogColor = skybox.GetColor(_color);
        while (time <= _fadeDuration) {
            LerpFog(time, defaultFogColor);
            skybox.SetColor(_color, Color.Lerp(defaultFogColor, Color.black, time / _fadeDuration));
            RenderSettings.skybox = skybox;
            time += Time.deltaTime;
            yield return null;
        }

        if (_ownPlayer != null && _ownPlayer.CurrentPillar != null) {
            _missionBriefingPillar = (_ownPlayer.CurrentPillar, _ownPlayer.CurrentPillar.GlassPillar);
            _ownPlayer.CurrentPillar.GlassPillar = true;
        }

        DeactivateSceneObjects();
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator) {
            GameObject adminCam = InstantiateWrapper.InstantiateWithMessage(_vCam, Vector3.zero, Quaternion.identity);
            SetMissionBriefingPosition(adminCam.transform);
        }

        yield return new WaitForSeconds(0.5f);
        StartMissionBriefing(matchDescription,gameMode);
        _mbCoroutine = null;
    }

    private void LerpFog(float time, Color defaultFogColor) {
        RenderSettings.fogStartDistance = Mathf.Lerp(_startFogDistance, _fogStartDistance, time / _fadeDuration);
        RenderSettings.fogEndDistance = Mathf.Lerp(_endFogDistance, _fogEndDistance, time / _fadeDuration);
        RenderSettings.fogColor = Color.Lerp(defaultFogColor, Color.black, time / _fadeDuration);
    }

    private void StartMissionBriefing(MatchDescription matchDescription, GameMode gameMode) {
        if (!_missionBriefingAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            Debug.LogWarning("Mission Briefing is already running!");
            return;
        }

        if (matchDescription.MissionBriefingPrefab == null) {
            Debug.LogError("Mission Briefing Prefab for the current match is missing! Abort Briefing");
            return;
        }

        _isRunning = true;
        CurrentMatchDescription = matchDescription;
        SetUpMissionBriefingForMatch(CurrentMatchDescription, gameMode);
        _announcer.Announce(matchDescription.GameMode);
    }

    private void SetUpMissionBriefingForMatch(MatchDescription matchDescription, GameMode gameMode) {
        _missionBriefingAnimator.SetTrigger(_startTriggerHash);
        _mapText.text = "MAP: " + matchDescription.MapName.ToUpper();
        _modeText.text = "MODE: " + gameMode.ToString().ToUpper();
        _missionText.text = _missionDescriptions[gameMode];
        _matchDependentMissionBriefing = InstantiateWrapper.InstantiateWithMessage(matchDescription.MissionBriefingPrefab,
            _stereoScreenShotParent.transform);
        _animationEventHandler.PinManager = _matchDependentMissionBriefing.GetComponentInChildren<TeamPinManager>();
        _announcer = _matchDependentMissionBriefing.transform.parent.parent.GetComponent<MissionBriefingAnnouncerSoundHandler>();
    }

    private void AbortMissionBriefing() {
        if (!_isRunning)
            return;
        if (_mbCoroutine != null)
            StopCoroutine(_mbCoroutine);
        ActivateSceneObjects();
        PillarWallManager.Singleton.GetAllWalls().ForEach(wall => wall.Reset());
        ResetRenderSettings();
        if (_missionBriefingAnimator.GetCurrentAnimatorStateInfo(0).IsName("MissionBriefing_PopUp"))
            _missionBriefingAnimator.SetTrigger(_abort);
        if (_missionBriefingPillar.pillar != null && !_missionBriefingPillar.wasGlass) {
            _missionBriefingPillar.pillar.GlassPillar = false;
        }

        FinishMissionBriefing();
    }

    private void ResetRenderSettings() {
        RenderSettings.skybox = _defaultSkyboxMaterial;
        RenderSettings.fogStartDistance = _startFogDistance;
        RenderSettings.fogEndDistance = _endFogDistance;
        RenderSettings.fogColor = _hubSceneBackgroundColor;
    }

    //Gets called by the animation
    public void FinishMissionBriefing() {
        if (_matchDependentMissionBriefing != null) {
            Destroy(_matchDependentMissionBriefing);
            _matchDependentMissionBriefing = null;
            _animationEventHandler.PinManager = null;
            _isRunning = false;
        }
    }

    private void ActivateSceneObjects() {
        _otherGameObjects.ForEach(objective => objective.SetActive(true));
        EnableComponentsFromChildren();
        ResetComponentLists();
    }

    private void DeactivateSceneObjects() {
        _otherGameObjects.ForEach(objective => {
            if (objective != null) objective.SetActive(false);
        });
        _hubLanes.Where(hubLane => SharedControllerType.IsAdmin || SharedControllerType.Spectator || hubLane.Player == null || !hubLane.Player.IsMe)
            .ForEach(hubLane => CollectComponentsToToggleFromChildren(hubLane.gameObject));
        HubLaneControllerBase localPlayerHubLane = _hubLanes.FirstOrDefault(hubLane => hubLane.Player != null && hubLane.Player.IsMe);
        if (localPlayerHubLane != null) {
            localPlayerHubLane.Pillars.ForEach(pillar => {
                if (!pillar.IsOccupied) CollectComponentsToToggleFromChildren(pillar.gameObject);
            });
        }

        DisableComponentsFromChildren();
    }

    private void CollectComponentsToToggleFromChildren(GameObject gO) {
        _renderers.AddRange(gO.GetComponentsInChildren<Renderer>());
        _canvases.AddRange(gO.GetComponentsInChildren<Canvas>());
        _textMeshPro.AddRange(gO.GetComponentsInChildren<TextMeshProUGUI>());
    }

    private void ResetComponentLists() {
        _renderers.Clear();
        _canvases.Clear();
        _textMeshPro.Clear();
    }

    private void DisableComponentsFromChildren() {
        _renderers.ForEach(meshRenderer => meshRenderer.enabled = false);
        _canvases.ForEach(canvas => canvas.enabled = false);
        _textMeshPro.ForEach(text => text.enabled = false);
    }

    private void EnableComponentsFromChildren() {
        _renderers.ForEach(meshRenderer => meshRenderer.enabled = true);
        _canvases.ForEach(canvas => canvas.enabled = true);
        _textMeshPro.ForEach(text => text.enabled = true);
    }
}