using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;

/// <summary>
/// Behaviour to control the Saturation effect depending on the pause status and the player position relative
/// to the chaperone bounds.
/// </summary>
public class SaturationEffectController : MonoBehaviour {
    [SerializeField, Tooltip("Shared boolean that is true when the game is paused")]
    private SharedBool _gamePaused;

    [SerializeField,
     Tooltip(
         "Reference to saturation image effect on players camera witch we want to fade in or out when player collides with objects/chaperone bounds.")]
    private Saturation _saturationEffect;

    [SerializeField,
     Tooltip(
         "Min value (X) will be set if saturation effect is fully faded out and Max (Y) if effect is fully faded in.")]
    private Vector2 _minMaxSaturationEffectValues;

    [SerializeField, Tooltip("Duration of the fade in/out animation.")]
    private float _fadeTime;

    // coroutine used to fade in/out image effects
    private Coroutine _fadeAnimationCoroutine;
    private IPlayer _player;

    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
        if (_player == null) {
            Debug.LogWarning("Player not found");
            enabled = false;
        }

        if (PlayerRigBase.GetInstance(out var playerRig) && playerRig.TryGetPlayerRigTransform(PlayerRigTransformOptions.Head, out var head)) {
            _saturationEffect = head.GetComponent<Saturation>();
        }
    }

    private void OnEnable() {
        if (_player != null) {
            _player.OutOfChaperoneStateChanged += UpdateEffect;
            _player.InTowerStateChanged += UpdateEffect;
        }

        _gamePaused.ValueChanged += UpdateEffect;
    }

    private void OnDisable() {
        if (_player != null) {
            _player.OutOfChaperoneStateChanged -= UpdateEffect;
            _player.InTowerStateChanged -= UpdateEffect;
        }

        _gamePaused.ValueChanged -= UpdateEffect;
    }

    private void Start() {
        ActivateEffects(_gamePaused || _player.IsOutOfChaperone || _player.IsInTower);
    }

    private void UpdateEffect(object sender, bool value) {
        ActivateEffects(_gamePaused || _player.IsOutOfChaperone || _player.IsInTower);
    }

    /// <summary>
    /// Activate or deactivate image effects.
    /// </summary>
    /// <param name="setActive">If true effects will be activated, if false effects will be deactivated.</param>
    private void ActivateEffects(bool setActive) {
        if (_fadeAnimationCoroutine != null)
            StopCoroutine(_fadeAnimationCoroutine);

        if (_saturationEffect != null)
        {
            _fadeAnimationCoroutine =
                StartCoroutine(LerpAnimationHelper.PlayFadeAnimation(setActive, _fadeTime, SetImageEffectValues,
                    EnableImageEffects));
        }
        else
        {
            Debug.LogError("SaturationEffectController.ActivateEffects - Saturation Effect not set");
            enabled = false; // disable the controller to avoid further spam
        }
    }

    /// <summary>
    /// Set parameters of image effects according to current state (position in) fade in/out animation.
    /// </summary>
    /// <param name="normalizedAnimationPosition">Position in current fade in/out animation. Fadein animation goes from 0 to 1, fadeout from 1 to 0.</param>
    private void SetImageEffectValues(float normalizedAnimationPosition) {
        // set saturation value
        if (_saturationEffect != null) {
            _saturationEffect.Value = Mathf.Lerp(_minMaxSaturationEffectValues.x, _minMaxSaturationEffectValues.y,
                normalizedAnimationPosition);
        }
        else
            Debug.LogError("Failed to set saturation");
    }

    /// <summary>
    /// Enable or disable image effect components.
    /// </summary>
    /// <param name="enable">If true components will be enabled, if false disabled.</param>
    private void EnableImageEffects(bool enable) {
        if (_saturationEffect != null)
            _saturationEffect.enabled = enable;
    }
}