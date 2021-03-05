using System.Collections;
using TowerTag;
using UnityEngine;

public sealed class LocalHealthVisuals : MonoBehaviour, IHealthVisuals {
    [SerializeField] private FloatVisuals _healthVisuals;

    [SerializeField] private ColorFaderPostFX _colorFader;

    [SerializeField] private CameraFilterPack_FX_Glitch1 _glitchEffect;

    [SerializeField] private string _hitSoundName;

    [SerializeField] private AudioSource _hitSource;

    [SerializeField] private string _deathSoundName;

    [SerializeField] private AudioSource _deathSource;

    [SerializeField] private HitGameAction _hitGameAction;

    [SerializeField] private HitRadarController _hitRadarController;
    private Coroutine _blackoutCoroutine;

    private void Awake() {
        InitSound(_hitSoundName, _hitSource);
        InitSound(_deathSoundName, _deathSource);

        if (PlayerRigBase.GetInstance(out var playerRig) && playerRig.TryGetPlayerRigTransform(PlayerRigTransformOptions.Head, out var head)) {
            _colorFader = head.GetComponent<ColorFaderPostFX>();
            _glitchEffect = head.GetComponent<CameraFilterPack_FX_Glitch1>();
            _hitRadarController = head.GetComponent<HitRadarController>();
        }
    }

    private void OnEnable() {
        _hitGameAction.PlayerGotHit += PlayLocalPlayerHitSound;
    }

    private void OnDisable() {
        _hitGameAction.PlayerGotHit -= PlayLocalPlayerHitSound;
    }

    public void OnHealthChanged(PlayerHealth sender, int newValue, IPlayer other, byte colliderType) {
        if (!sender.Player.IsMe) return;
        UpdateHealthBar(sender.HealthFraction);
    }


    private void UpdateHealthBar(float healthFraction) {
        if (_healthVisuals != null)
            _healthVisuals.SetValue(healthFraction);
    }

    // only triggered locally (on local client for local damageModel)
    public void OnTookDamage(PlayerHealth playerHealth, IPlayer other) {
        if (!playerHealth.Player.IsMe) return;
        if(_hitRadarController != null)
            _hitRadarController.TriggerFrontalVisuals();
        if (_hitSource != null)
            _hitSource.Play();
    }

    private void PlayLocalPlayerHitSound(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        if (targetPlayer.IsMe) {
            if (_hitSource != null)
                _hitSource.Play();
        }
    }

    public void OnPlayerDied(PlayerHealth playerHealth, IPlayer other, byte colliderType) {
        if (!playerHealth.Player.IsMe) return;
        // play death sound
        if (_deathSource != null)
            _deathSource.Play();

        // show death effect
        if (_blackoutCoroutine != null)
            StopCoroutine(_blackoutCoroutine);

        _blackoutCoroutine = StartCoroutine(Blackout(0.1f, 2));
    }

    private static void InitSound(string soundName, AudioSource source) {
        Sound sound = SoundDatabase.Instance.GetSound(soundName);
        if (sound == null) {
            Debug.LogWarning("Could not find Sound (" + soundName + ") in Database!");
            return;
        }

        sound.InitSource(source);
    }

    private IEnumerator Blackout(float timeout, float fadeInTime) {
        EnableGlitchEffects(true);

        yield return new WaitForSeconds(timeout);

        if (_colorFader != null && BalancingConfiguration.Singleton.UseFadeBlackWhenDie) {
            //SteamVR_Fade.View(Color.clear, fadeInTime);
            float startTime = Time.realtimeSinceStartup;
            float delta = 0;

            if (fadeInTime > 0) {
                while (delta <= 1f) {
                    delta = (Time.realtimeSinceStartup - startTime) / fadeInTime;
                    _colorFader.FadeFactor = 1f - delta;
                    yield return new WaitForEndOfFrame();
                }
            }
        } else {
            yield return new WaitForSeconds(fadeInTime);
        }

        EnableGlitchEffects(false);
    }

    private void EnableGlitchEffects(bool setActive) {
        if (_glitchEffect != null && BalancingConfiguration.Singleton.UseGlitchEffectWhenDie) {
            _glitchEffect.enabled = setActive;
        }

        if (_colorFader != null && BalancingConfiguration.Singleton.UseFadeBlackWhenDie) {
            _colorFader.FadeColor = Color.black;
            _colorFader.FadeFactor = setActive ? 1 : 0;
            _colorFader.enabled = setActive;
        }
    }

    private void OnDestroy() {
        if (_blackoutCoroutine != null) {
            StopCoroutine(_blackoutCoroutine);
        }
    }
}
