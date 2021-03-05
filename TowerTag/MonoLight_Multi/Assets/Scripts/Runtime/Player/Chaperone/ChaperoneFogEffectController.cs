using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Behaviour to change the scene Fog settings between the default settings for the scene and the
/// out-of-chaperone fog settings
/// </summary>
public class ChaperoneFogEffectController : MonoBehaviour {
    [SerializeField, Tooltip("The default fog settings for the current scene")]
    private SharedFogSettings _sceneFogSettings;

    [SerializeField, Tooltip("These settings are applied when user leaves the chaperone")]
    private SharedFogSettings _outOfChaperoneFogSettings;

    [SerializeField, Tooltip("Duration of the fade in/out animation.")]
    private float _fadeTime;

    private bool _isActive;
    private IPlayer _player;

    // coroutine used to fade in/out image effects
    private Coroutine _fadeAnimationCoroutine;

    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
        if (!SharedControllerType.IsPlayer || _player == null) {
            enabled = false;
        }
    }

    private void OnEnable() {
        _player.OutOfChaperoneStateChanged += ActivateEffects;
        _player.InTowerStateChanged += ActivateEffects;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        if (_player != null) {
            _player.OutOfChaperoneStateChanged -= ActivateEffects;
            _player.InTowerStateChanged -= ActivateEffects;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start() {
        ActivateEffects(_player, _player.IsOutOfChaperone || _player.IsInTower);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
        ActivateEffects(_player, _isActive);
    }

    /// <summary>
    /// Activate or deactivate image effects.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="value">Value of the observed boolean. Effect activity depends on multiple values.</param>
    private void ActivateEffects(IPlayer player, bool value) {
        _isActive = _player.IsOutOfChaperone || _player.IsInTower;
        FogSettings fogSettings = _isActive ? _outOfChaperoneFogSettings.FogSettings : _sceneFogSettings.FogSettings;
        RenderSettings.fog = fogSettings.FogEnabled;
        RenderSettings.fogMode = fogSettings.FogMode;
        RenderSettings.skybox = fogSettings.SkyBox;

        if (_fadeAnimationCoroutine != null)
            StopCoroutine(_fadeAnimationCoroutine);
        if (gameObject.activeInHierarchy) {
            _fadeAnimationCoroutine =
                StartCoroutine(LerpAnimationHelper.PlayFadeAnimation(_isActive, _fadeTime, SetImageEffectValues));
        }
    }

    /// <summary>
    /// Set parameters of image effects according to current state (position in) fade in/out animation.
    /// </summary>
    /// <param name="normalizedAnimationPosition">Position in current fade in/out animation. Fadein animation goes from 0 to 1, fadeout from 1 to 0.</param>
    private void SetImageEffectValues(float normalizedAnimationPosition) {
        RenderSettings.fogStartDistance = Mathf.Lerp(
            _sceneFogSettings.FogSettings.FogStartDistance,
            _outOfChaperoneFogSettings.FogSettings.FogStartDistance, normalizedAnimationPosition);
        RenderSettings.fogEndDistance = Mathf.Lerp(
            _sceneFogSettings.FogSettings.FogEndDistance,
            _outOfChaperoneFogSettings.FogSettings.FogEndDistance, normalizedAnimationPosition);
        RenderSettings.fogDensity = Mathf.Lerp(
            _sceneFogSettings.FogSettings.FogDensity,
            _outOfChaperoneFogSettings.FogSettings.FogDensity, normalizedAnimationPosition);
        RenderSettings.fogColor = Color.Lerp(
            _sceneFogSettings.FogSettings.FogColor,
            _outOfChaperoneFogSettings.FogSettings.FogColor, normalizedAnimationPosition);
    }
}