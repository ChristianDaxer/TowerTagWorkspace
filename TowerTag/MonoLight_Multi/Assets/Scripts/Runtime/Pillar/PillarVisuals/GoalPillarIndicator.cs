using System.Collections;
using System.Linq;
using TowerTag;
using UnityEngine;
using VLB;

[RequireComponent(typeof(AudioSource))]
public class GoalPillarIndicator : MonoBehaviour {
    [SerializeField] private Pillar _owningPillar;
    [SerializeField] private Renderer[] _emissiveLight;
    [SerializeField] private VolumetricLightBeam[] _lightBeams;

    [SerializeField, Tooltip("Color blink speed during claim scales with the claim value and this factor")]
    private float _colorBlinkSpeedFactor = 5;

    [SerializeField, Tooltip("The speed of the blinking at the beginning of a round")]
    private int _intensityBlinkSpeed = 2;

    private Pillar[] _allGoalPillars;
    private AudioSource _audioSource;
    private float _currentClaimValue;
    private ParticleSystem.MainModule _main;
    private Color _currentColor;
    private float _blinkSpeed;
    private Coroutine _blinkColorCoroutine;
    [SerializeField] private Gradient _lerpGradient;
    private GradientColorKey[] _colorKeys;
    private MaterialPropertyBlock _propertyBlock;

    private readonly GradientAlphaKey[] _alphaKeys = {
        new GradientAlphaKey(1, 0),
        new GradientAlphaKey(1, 1)
    };

    private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");

    //Intensity Coroutine
    private float _lowestIntensityValue = 0.05f;
    private int _maxBlinkRepetitions = 2;
    private Coroutine _intensityCoroutine;

    public void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        if(_owningPillar != null)
            OnOwningTeamChanged(_owningPillar, TeamID.Neutral, _owningPillar.OwningTeamID, new IPlayer[0]);
    }

    private void OnEnable() {
        _owningPillar.ChargeSet += OnClaimStatusSet;
        _owningPillar.OwningTeamChanged += OnOwningTeamChanged;
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.RoundStartingAt += StartBlinkIntensity;
            GameManager.Instance.CurrentMatch.StartingAt += StartBlinkIntensity;
        }
    }

    private void OnDisable() {
        _owningPillar.ChargeSet -= OnClaimStatusSet;
        _owningPillar.OwningTeamChanged -= OnOwningTeamChanged;
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.RoundStartingAt -= StartBlinkIntensity;
            GameManager.Instance.CurrentMatch.StartingAt -= StartBlinkIntensity;
        }
    }

    private void Start() {
        _allGoalPillars = PillarManager.Instance.GetAllGoalPillarsInScene();
        _audioSource = GetComponent<AudioSource>();
        _emissiveLight[0].material.EnableKeyword("_EMISSION");
        SetBeamColorByTeamID(_owningPillar.OwningTeamID);
    }

    private void OnOwningTeamChanged(Claimable claimable, TeamID oldTeamID, TeamID newTeamID, IPlayer[] newOwner) {
        SetBeamColorByTeamID(newTeamID);
        if (_blinkColorCoroutine != null)
            StopCoroutine(_blinkColorCoroutine);
        _blinkColorCoroutine = null;
    }

    private void OnClaimStatusSet(Chargeable chargeable, TeamID teamID, float value) {
        _currentClaimValue = value;
        if (_blinkColorCoroutine == null)
            _blinkColorCoroutine = StartCoroutine(BlinkColors(teamID));

        if (value <= 0.01f) {
            _lightBeams.ForEach(lightBeam => lightBeam.color = _currentColor);
            if (_blinkColorCoroutine != null)
                StopCoroutine(_blinkColorCoroutine);

            _blinkColorCoroutine = null;
        }
    }

    private void StartBlinkIntensity(IMatch match, int time) {
        if (_intensityCoroutine != null)
            StopCoroutine(_intensityCoroutine);

        _intensityCoroutine = StartCoroutine(BlinkIntensity(match));
    }

    private IEnumerator BlinkIntensity(IMatch match) {
        float lerpValue = 0;
        float startIntensityBeam = _lightBeams[0].intensityGlobal;
        float startIntensityBeam2 = _lightBeams[1].intensityGlobal;
        var blinkRepetitionAfterRoundStart = 0;
        var direction = 1;

        while (enabled) {
            if (lerpValue <= 0 && match.IsActive) {
                blinkRepetitionAfterRoundStart++;
            }

            lerpValue += Time.deltaTime * _intensityBlinkSpeed * direction;
            _lightBeams[0].intensityGlobal = Mathf.Lerp(startIntensityBeam, _lowestIntensityValue, lerpValue);
            _lightBeams[1].intensityGlobal = Mathf.Lerp(startIntensityBeam2, _lowestIntensityValue, lerpValue);

            if (lerpValue >= 1 && direction > 0 || lerpValue <= 0 && direction < 0) {
                direction = -direction;
            }

            if (blinkRepetitionAfterRoundStart == _maxBlinkRepetitions && lerpValue <= 0) {
                _intensityCoroutine = null;
                _lightBeams[0].intensityGlobal = startIntensityBeam;
                _lightBeams[1].intensityGlobal = startIntensityBeam2;
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator BlinkColors(TeamID claimingTeamID) {
        bool lastPillar = _allGoalPillars.Count(pillar => pillar.OwningTeamID != claimingTeamID) == 1;
        ITeam claimingTeam = TeamManager.Singleton.Get(claimingTeamID);
        ITeam owningTeam = TeamManager.Singleton.Get(_owningPillar.OwningTeamID);
        if(claimingTeam == null || owningTeam == null)
            yield break;

        float lerpValue = 0;
        if (_owningPillar.OwningTeamID == TeamID.Neutral) {
            _colorKeys = new[] {
                new GradientColorKey(owningTeam.Colors.Main, 0),
                new GradientColorKey(claimingTeam.Colors.Main, 1)
            };
        }
        else {
            _colorKeys = new[] {
                new GradientColorKey(owningTeam.Colors.Main, 0),
                new GradientColorKey(TeamManager.Singleton.TeamNeutral.Colors.Main, 0.5f),
                new GradientColorKey(claimingTeam.Colors.Main, 1)
            };
        }

        _lerpGradient.SetKeys(_colorKeys, _alphaKeys);
        while (enabled) {
            if (!_audioSource.isPlaying && lastPillar)
                _audioSource.Play();
            lerpValue += Time.deltaTime * _currentClaimValue * _colorBlinkSpeedFactor;
            Color newColor = _lerpGradient.Evaluate(lerpValue);
            _lightBeams.ForEach(lightBeam => lightBeam.color = newColor);

            if (lerpValue >= 1 && _colorBlinkSpeedFactor > 0 || lerpValue <= 0 && _colorBlinkSpeedFactor < 0)
                _colorBlinkSpeedFactor = -_colorBlinkSpeedFactor;
            yield return null;
        }
    }

    private void SetBeamColorByTeamID(TeamID teamID) {
        if (TeamManager.Singleton == null) return;
        ITeam team = TeamManager.Singleton.Get(teamID);
        _currentColor = team.Colors.Effect;
        _lightBeams.ForEach(lightBeam => lightBeam.color = _currentColor);
        _propertyBlock = _propertyBlock ?? new MaterialPropertyBlock();
        _emissiveLight[0].GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_emissionColor, team.Colors.Emissive);
        _emissiveLight[0].SetPropertyBlock(_propertyBlock);
    }
}