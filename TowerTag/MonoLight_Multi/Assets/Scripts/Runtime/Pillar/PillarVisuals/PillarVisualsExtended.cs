using System;
using System.Linq;
using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using SineVFX;

public class PillarVisualsExtended : PillarVisuals {
    [Space] [Header("References")] [SerializeField]
    private GameObject _pillarTopVisualParent;

    [SerializeField] private GameObject _claimVisualsParent;
    [SerializeField] private GameObject _claimBlockerShield;
    [SerializeField] private ForceFieldController _claimBlockerShieldController;
    [SerializeField] private Renderer[] _rendererToClaimTint;
    [SerializeField] private Renderer[] _rendererToTint;
    [SerializeField] private Light[] _lightsToTint;

    [Header("Color Properties")] [SerializeField]
    private string[] _colorPropertyNames;

    [SerializeField] private float _shadeBackgroundColorThreshold = 0.5f;
    [SerializeField] private Vector2 _shadeBackgroundColorFactorMinMax = new Vector2(0.5f, 1f);

    [Header("Visualization Extrapolation and Error Correction")]
    [SerializeField, Tooltip("Deviations of the visualization from the setpoint value are corrected by accelerating " +
                             "the visualization towards the setpoint by a maximum of this value.")]
    private float _maxAcceleration;

    [SerializeField, Tooltip("Damps the visualization correction in case it would increase the error " +
                             "in visualization speed.")]
    private float _correctionDamping;

    [SerializeField, Tooltip("When visualization deviates from the setpoint value more than this, " +
                             "it is directly set to the setpoint value.")]
    private float _visualizationJumpThreshold;

    [SerializeField] private Renderer[] _teamTintedRenderers;
    [SerializeField] private Renderer[] _highlightRenderers;
    [SerializeField] private Light[] _contrastLights;

    private readonly int _claimValueID = Shader.PropertyToID("_ClaimValue");
    private readonly int _claimColorID = Shader.PropertyToID("_ClaimColor");
    private readonly int _tintColorID = Shader.PropertyToID("_TintColor");

    private readonly string _claimLocalToWorld = "_ClaimLocalToWorld";

    private int[] _colorPropertyIDs = {
        Shader.PropertyToID("_Color"),
        Shader.PropertyToID("_EmissionColor"),
        Shader.PropertyToID("_ClaimColor")
    };

    public bool IsClaiming { get; private set; }
    private TeamID _currentClaimingTeamID = TeamID.Neutral;
    private Color _claimStartBackgroundColor;
    private float _lerpSpeed;
    private float _visualizedClaimValue;
    private IPlayer _ownPlayer;

    private (float time, float value) _latestClaimUpdate;
    private (float time, float value) _previousClaimUpdate;
    private float _setpointClaimSpeed;
    private float _visualizedClaimSpeed;

    private const float FloatTolerance = 0.001f;

    private void Awake() {
        SetOwningTeam(Pillar, TeamID.Neutral, Pillar.OwningTeamID, new IPlayer[] { });
        InitHighlight();

        // Init ClaimingVisual visibility
        _isActivatedByPillarNeighbour = false;
        _pillarTopVisualParent.SetActive(CanSetActive());
        SetClaimBlockerShieldActive(CanClaimBlockerShieldSetActive());
        _latestClaimUpdate = (Time.time, 0);
        _previousClaimUpdate = (Time.time, 0);
    }

    public void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        _teamTintedRenderers = _pillarTopVisualParent.GetComponentsInChildren<Renderer>(true);
        _highlightRenderers = _highlightVisualsParent.GetComponentsInChildren<Renderer>(true);
        _contrastLights = _pillarTopVisualParent.GetComponentsInChildren<Light>(true);
        _colorPropertyIDs = new int[_colorPropertyNames.Length];
        for (var i = 0; i < _colorPropertyNames.Length; i++) {
            _colorPropertyIDs[i] = Shader.PropertyToID(_colorPropertyNames[i]);
        }

        if (TeamManager.Singleton != null)
            SetOwningTeam(Pillar, TeamID.Neutral, Pillar.OwningTeamID, new IPlayer[0]);
    }

    private void Start() {
        _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (_ownPlayer != null) {
            _ownPlayer.GunController.TeleportDenied += OnTeleportDenied;
        }
    }

    private void Update() {
        UpdateHighlight();
        UpdateClaimVisuals();
    }

    private void OnDestroy() {
        if (_ownPlayer != null) {
            _ownPlayer.GunController.TeleportDenied -= OnTeleportDenied;
        }
    }

    // change colors in:
    //          - change color on PillarTopPrefabs (Highlight & ClaimVisuals)           *
    //          - change color values in light stripes of pillars                       *
    //          - change color values in lights on pillars                              *
    //          - change claimShader backgroundColor and reset claimValue (on shader) to zero
    protected override void SetOwningTeam(Claimable claimable, TeamID oldTeamID, TeamID newTeamID,
        IPlayer[] attachedPlayers) {
        ITeam newTeam = TeamManager.Singleton.Get(newTeamID);

        if (newTeam != null) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint, _claimValueID, 1);
            ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(_rendererToClaimTint, _claimLocalToWorld);
            ColorChanger.ChangeColorInRendererComponents(_rendererToClaimTint,
                newTeamID == TeamID.Neutral ? newTeam.Colors.Dark : newTeam.Colors.Emissive,
                _tintColorID);
            _claimStartBackgroundColor = newTeamID == TeamID.Neutral ? newTeam.Colors.Dark : newTeam.Colors.Emissive;
            TintInTeamColors(newTeam);
            TintClaimBlockerShieldInTeamColors(newTeam);
        }

        if (attachedPlayers.Any(p => p.IsMe)) StartClaimFinishedAnimation();
    }

    protected override void OnPlayerDetached(Chargeable chargeable, IPlayer player) {
        if (Pillar.AttachedPlayers.Count == 0) {
            SetCurrentClaim(Pillar, Pillar.CurrentCharge.teamID, Pillar.CurrentCharge.value);
        }
    }

    protected override void SetCurrentClaim(Chargeable chargeable, TeamID teamID, float chargeValue) {
        bool valueChanged = Math.Abs(_latestClaimUpdate.value - chargeValue) > FloatTolerance
                            || _currentClaimingTeamID != teamID;
        CalculateSpeedAndClaimValue(chargeValue);
        if (!valueChanged) return;
        // deactivate Claim
        if (teamID == TeamID.Neutral && IsClaiming)
            ActivateClaimVisuals(false);
        if (teamID != TeamID.Neutral && !IsClaiming)
            ActivateClaimVisuals(true);

        // Start Claim
        if (_currentClaimingTeamID != teamID && teamID != TeamID.Neutral) {
            ITeam team = TeamManager.Singleton.Get(teamID);
            ColorChanger.ChangeColorInRendererComponents(_rendererToClaimTint, team.Colors.Emissive,
                _claimColorID);
        }

        if (teamID == TeamID.Neutral) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint, _claimValueID, 0);
            ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(_rendererToClaimTint, _claimLocalToWorld);
        }

        _currentClaimingTeamID = teamID;
    }

    private void CalculateSpeedAndClaimValue(float newValue) {
        // time at which the update was allegedly sent.
        float updateTime = Time.time;
        if (!PhotonNetwork.IsMasterClient) {
            updateTime -= HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                PillarManagerStateSyncHelper.LastPropertyUpdateTimestamp, PhotonNetwork.ServerTimestamp);
        }

        _previousClaimUpdate = _latestClaimUpdate;
        _latestClaimUpdate = (updateTime, newValue);
        float timeBetweenUpdates = _latestClaimUpdate.time - _previousClaimUpdate.time;
        if (timeBetweenUpdates > 0)
            _setpointClaimSpeed = (_latestClaimUpdate.value - _previousClaimUpdate.value) / timeBetweenUpdates;
    }

    private void UpdateClaimVisuals() {
        // the best guess of the actual current claim
        float setpointClaimValue =
            Mathf.Clamp01(_latestClaimUpdate.value + (Time.time - _latestClaimUpdate.time) * _setpointClaimSpeed);

        // linear extrapolation of visualization
        float extrapolatedValue = _visualizedClaimValue + _visualizedClaimSpeed * Time.deltaTime;

        // deviation of extrapolated value from setpoint value
        float error = extrapolatedValue - setpointClaimValue;

        float lastVisualizedClaimValue = _visualizedClaimValue;

        // immediately set visualization and speed to setpoint values
        if (_visualizedClaimValue <= 0 && setpointClaimValue > 0 || Mathf.Abs(error) > _visualizationJumpThreshold) {
//            Debug.Log("Setting values directly: " +
//                      $"visualized {_visualizedClaimValue} " +
//                      $"| speed {_visualizedClaimSpeed} " +
//                      $"| setpoint {setpointClaimValue} " +
//                      $"| setpoint speed {_setpointClaimSpeed}");
            _visualizedClaimValue = setpointClaimValue;
            _visualizedClaimSpeed = _setpointClaimSpeed;
        }
        // extrapolate value and correct for errors within bounds of maximum acceleration and damping
        else {
            float speedError = _visualizedClaimSpeed - _setpointClaimSpeed;
            float maxCorrection = _maxAcceleration * Time.deltaTime * Time.deltaTime;
            float correction = Mathf.Clamp(-error, -maxCorrection, maxCorrection);
            float damping = speedError * correction > 0 // correction increases speed error
                ? Mathf.Exp(-Mathf.Abs(speedError) * _correctionDamping)
                : 1;
            correction *= damping;
            float nextValue = extrapolatedValue + correction;
            _visualizedClaimSpeed = (nextValue - _visualizedClaimValue) / Time.deltaTime;
            _visualizedClaimValue = Mathf.Clamp01(nextValue);
//            UnityEngine.Debug.Log($"Setpoint: {setpointClaimValue:0.##} " +
//                                  $"| Extrapolated: {extrapolatedValue:0.##} " +
//                                  $"| Error {nextValue - setpointClaimValue:0.####} " +
//                                  $"| Correction {correction:0.#####} " +
//                                  $"| Damping {damping:0.##} " +
//                                  $"| Next {nextValue:0.##} " +
//                                  $"| Setpoint Speed {_setpointClaimSpeed:0.###} " +
//                                  $"| Visualized Speed {_visualizedClaimSpeed:0.###} " +
//                                  $"| SpeedError {_visualizedClaimSpeed - _setpointClaimSpeed:0.#####} " +
//                                  $"| deltaTime {Time.deltaTime:0.####}");
        }

        if (!IsClaiming && Pillar.OwningTeamID != TeamID.Neutral)
            _visualizedClaimValue = 1;

        if (_visualizedClaimValue < FloatTolerance)
            _visualizedClaimValue = 0;

        if ((_visualizedClaimValue > 0 || lastVisualizedClaimValue > 0) &&
            !_visualizedClaimValue.Equals(lastVisualizedClaimValue)) {
            UpdateClaimValueInShader(_visualizedClaimValue);
        }


        // use _rendererToClaimTint array to show Blink Animations only on claim bar or _rendererToTint to show animations also on Platform Light Edge
        if (_claimFinishedAnimation.IsPlaying) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint,
                _claimShaderTexIntensityMultiplyPropertyID, _claimFinishedAnimation.ResultingValues[0]);
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint,
                _claimShaderTexIntensityAdditivePropertyID, _claimFinishedAnimation.ResultingValues[1]);
            ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(_rendererToClaimTint, _claimLocalToWorld);
        }
        else if (_claimAbortedAnimation.IsPlaying) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint,
                _claimShaderTexIntensityMultiplyPropertyID, _claimAbortedAnimation.ResultingValues[0]);
            ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint,
                _claimShaderTexIntensityAdditivePropertyID, _claimAbortedAnimation.ResultingValues[1]);
            ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(_rendererToClaimTint, _claimLocalToWorld);
        }
    }

    private void UpdateClaimValueInShader(float claimValue) {
        // Update Claim
        ColorChanger.SetCustomFloatPropertyInRenderers(_rendererToClaimTint, _claimValueID, claimValue);
        ColorChanger.SetCustomMatrixPropertyToRendererLocalToWorld(_rendererToClaimTint, _claimLocalToWorld);
        // shade BackgroundColor -> TODO: calculate these in ClaimShader
        if (claimValue <= _shadeBackgroundColorThreshold) {
            float shadeFactor = Mathf.Lerp(_shadeBackgroundColorFactorMinMax.y, _shadeBackgroundColorFactorMinMax.x,
                Mathf.InverseLerp(0, _shadeBackgroundColorThreshold, claimValue));
            Color currentBGColor = _claimStartBackgroundColor * shadeFactor;
            ColorChanger.ChangeColorInRendererComponents(_rendererToClaimTint, currentBGColor, _tintColorID);
        }
    }

    private void ActivateClaimVisuals(bool activate) {
        IsClaiming = activate;
        _visualizedClaimValue = 0;
        UpdateClaimValueInShader(0);

        Assert.AreNotEqual(_claimVisualsParent, null);
        Assert.AreNotEqual(_highlightVisualsParent, null);

        if (_claimVisualsParent != null)
            _claimVisualsParent.SetActive(activate);

        if (_deactivateHighlightVisualsWhenClaiming && _highlightVisualsParent != null) {
            //_highlightVisualsParent.SetActive(!activate);
            if (activate) {
                _interruptedHighlightByClaim = IsHighlighted;
                ShowHighlightIntern(false);
            }
            else {
                if (_interruptedHighlightByClaim) {
                    ShowHighlightIntern(true);
                }
            }
        }

        if (!activate) {
            ColorChanger.ChangeColorInRendererComponents(_rendererToClaimTint,
                Pillar.OwningTeam.Colors.Emissive, _claimColorID);
        }
    }

    private void TintInTeamColors(ITeam team) {
        ITeam opponentTeam = TeamManager.Singleton.Get(team.ID == TeamID.Neutral
            ? TeamID.Neutral
            : team.ID == TeamID.Fire
                ? TeamID.Ice
                : TeamID.Fire);
        ColorChanger.ChangeColorInRendererComponents(_teamTintedRenderers, team.Colors.Emissive, true);
        ColorChanger.ChangeColorInLightComponents(_contrastLights, opponentTeam.Colors.ContrastLights);
        ColorChanger.ChangeColorInLightComponents(_lightsToTint, opponentTeam.Colors.ContrastLights);

        if (_colorPropertyIDs != null) {
            foreach (int propertyID in _colorPropertyIDs) {
                ColorChanger.ChangeColorInRendererComponents(_rendererToTint, team.ID == TeamID.Neutral
                    ? team.Colors.Dark
                    : team.Colors.Emissive, propertyID, true);
            }
        }
    }

    private void TintClaimBlockerShieldInTeamColors(ITeam team) {
        if (_claimBlockerShield == null || _claimBlockerShieldController == null)
            return;

        // setup color gradle in team colors

        var gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        var colorKey = new GradientColorKey[3];
        colorKey[0].color = team.Colors.Main;
        colorKey[0].time = 0.2f;
        colorKey[1].color = team.Colors.Dark;
        colorKey[1].time = 0.3f;
        colorKey[2].color = team.Colors.Main;
        colorKey[2].time = 0.6f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        var alphaKey = new GradientAlphaKey[3];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.2f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 0.4f;
        alphaKey[2].alpha = 1.0f;
        alphaKey[2].time = 0.6f;

        gradient.SetKeys(colorKey, alphaKey);

        // set claim block shield color

        _claimBlockerShieldController.ProcedrualGradientRamp = gradient;
        //_claimBlockerShieldController.ProcedrualRampColorTint = team.Colors.Main;
    }


    // Highlighting

    [Space] [Header("Highlight")] [SerializeField]
    private bool _deactivateHighlightVisualsWhenClaiming;

    [SerializeField] private GameObject _highlightVisualsParent;
    [SerializeField] private float _minIntensityValue;
    [SerializeField] private float _maxIntensityValue = 1f;
    [SerializeField] private float _playbackSpeed = 1f;
    [SerializeField] private AnimationCurve _highlightIntensityCurve;
    [SerializeField] private AudioSource _highlightSoundSource;
    [SerializeField] private string _highlightSoundName;
    private Sound _highlightSound;
    public bool IsHighlighted { get; private set; }
    private bool _interruptedHighlightByClaim;
    private float _currentHighlightTimer;

    private void InitHighlight() {
        ChangeHighlightTransparency(_minIntensityValue);

        // Init Sound
        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play init highlight sound: no sound database");
            return;
        }

        _highlightSound = SoundDatabase.Instance.GetSound(_highlightSoundName);

        if (_highlightSound == null) {
            Debug.LogError("Highlight Sound is null");
            return;
        }

        _highlightSound.InitSource(_highlightSoundSource);
    }

    private void UpdateHighlight() {
        if (IsHighlighted) {
            // HighlightVisuals
            if (_highlightRenderers != null) {
                float value = Mathf.Lerp(_minIntensityValue, _maxIntensityValue,
                    _highlightIntensityCurve.Evaluate(_currentHighlightTimer));
                ChangeHighlightTransparency(value);
                _currentHighlightTimer = (_currentHighlightTimer + Time.deltaTime * _playbackSpeed) % 1f;
            }
        }
    }

    private void ShowHighlightIntern(bool highlight) {
        IsHighlighted = highlight;
        if (highlight) {
            _currentHighlightTimer = 0f;
        }
        else {
            _currentHighlightTimer = 0f;
            ChangeHighlightTransparency(_minIntensityValue);
        }
    }

    protected override void ShowHighlight(bool highlight) {
        ShowHighlightIntern(highlight);
        if (highlight) {
            _highlightSoundSource.Play();
        }
        else {
            _interruptedHighlightByClaim = false;
            _highlightSoundSource.Stop();
        }
    }

    private void ChangeHighlightTransparency(float transparency) {
        if (_highlightRenderers != null)
            ColorChanger.ChangeTransparencyInRendererComponents(_highlightRenderers, transparency);
    }

    #region switch visibility of PillarClaimVisuals

    protected override void OnClaimableStatusChanged(Claimable claimable, bool active) {
        SetPillarVisualsActive(CanSetActive());
        SetClaimBlockerShieldActive(CanClaimBlockerShieldSetActive());
    }

    // switch visibility of PillarClaimVisuals
    private bool _isActivatedByPillarNeighbour;

    // OccupancyChange
    protected override void OnOccupancyChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner) {
        if (TTSceneManager.Instance == null || TTSceneManager.Instance.IsInCommendationsScene)
            return;
        SetPillarVisualsActive(CanSetActive());
        SetClaimBlockerShieldActive(CanClaimBlockerShieldSetActive());
    }

    // called by local client to switch visibility on neighbourPillars
    public void SetActivatedByNeighbours(bool setActive) {
        _isActivatedByPillarNeighbour = setActive;
        SetPillarVisualsActive(_isActivatedByPillarNeighbour && Pillar.IsClaimable && !Pillar.IsOccupied);
        SetClaimBlockerShieldActive(CanClaimBlockerShieldSetActive());
    }

    private bool CanSetActive() {
        // On Admin, default pillar top visuals to active
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator) {
            return true;
        }

        return _isActivatedByPillarNeighbour && Pillar.IsClaimable && !Pillar.IsOccupied;
    }

    private bool CanClaimBlockerShieldSetActive() {
        // On Admin or local player == null, default blockers to inactive
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator || _ownPlayer == null)
            return false;

        return _isActivatedByPillarNeighbour
               && Pillar.IsClaimable
               && !Pillar.IsOccupied
               && Pillar.IsTeamBased
               && Pillar.OwningTeamID != _ownPlayer.TeamID;
    }

    public void SetPillarVisualsActive(bool setActive) {
        // activate/deaactivate visuals parent gameObject (parent of highlight/claimVisuals)
        if (_pillarTopVisualParent != null)
            _pillarTopVisualParent.SetActive(setActive);

        // Reset HighlightVisuals to start with min value if activated
        if (setActive)
            ChangeHighlightTransparency(_minIntensityValue);
    }

    private void SetClaimBlockerShieldActive(bool setActive) {
        if (_claimBlockerShield != null)
            _claimBlockerShield.SetActive(setActive);
    }

    #endregion

    #region Play BlinkAnimations

    [Header("Claim Blink Animations")] private readonly int _claimShaderTexIntensityMultiplyPropertyID = Shader.PropertyToID("_TextureFactor");

    [FormerlySerializedAs("textureIntensityMultiplyMin")] [SerializeField]
    private float _textureIntensityMultiplyMin;

    [FormerlySerializedAs("textureIntensityMultiplyZero")] [SerializeField]
    private float _textureIntensityMultiplyZero;

    [FormerlySerializedAs("textureIntensityMultiplyMax")] [SerializeField]
    private float _textureIntensityMultiplyMax;

    [Header("Texture Intensity Additive")] private readonly int _claimShaderTexIntensityAdditivePropertyID = Shader.PropertyToID("_TextureFactorOffset");

    [FormerlySerializedAs("textureIntensityAdditiveMin")] [SerializeField]
    private float _textureIntensityAdditiveMin;

    [FormerlySerializedAs("textureIntensityAdditiveZero")] [SerializeField]
    private float _textureIntensityAdditiveZero;

    [FormerlySerializedAs("textureIntensityAdditiveMax")] [SerializeField]
    private float _textureIntensityAdditiveMax;

    [FormerlySerializedAs("claimFinishedAnimation")] [SerializeField]
    private FloatArrayLerpAnimationNegativePositiveLerp _claimFinishedAnimation;

    [FormerlySerializedAs("claimAbortedAnimation")] [SerializeField]
    private FloatArrayLerpAnimationNegativePositiveLerp _claimAbortedAnimation;

    private void OnTeleportDenied(IPlayer player, Pillar target) {
        if (target == Pillar) {
            if (_currentClaimingTeamID != TeamID.Neutral && !_claimAbortedAnimation.IsPlaying) {
                _claimFinishedAnimation.StopAnimation();
                _claimAbortedAnimation.StartAnimation(
                    new[] {_textureIntensityMultiplyZero, _textureIntensityAdditiveZero},
                    new[] {_textureIntensityMultiplyMin, _textureIntensityAdditiveMin},
                    new[] {_textureIntensityMultiplyMax, _textureIntensityAdditiveMax});
            }
        }
    }

    private void StartClaimFinishedAnimation() {
        _claimAbortedAnimation.StopAnimation();
        _claimFinishedAnimation.StartAnimation(
            new[] {_textureIntensityMultiplyZero, _textureIntensityAdditiveZero},
            new[] {_textureIntensityMultiplyMin, _textureIntensityAdditiveMin},
            new[] {_textureIntensityMultiplyMax, _textureIntensityAdditiveMax});
    }

    #endregion
}