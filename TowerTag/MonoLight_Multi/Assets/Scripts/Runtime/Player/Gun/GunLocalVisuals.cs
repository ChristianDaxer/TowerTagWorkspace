using UnityEngine;

/// <summary>
/// Controls the audio-visual feedback of the local player gun.
/// </summary>
public sealed class GunLocalVisuals : MonoBehaviour, IGunVisuals {
    [SerializeField] private FloatVisuals _gunEnergyVisual;
    [SerializeField] private AudioSource _source;
    [SerializeField] private string _chargeSoundName;
    [SerializeField] private string _fullyChargedSoundName;

    private float _lastEnergyValue;
    private bool _isCharging;
    private Sound _chargeSound;
    private Sound _fullyChargedSound;
    private GunController _gunController;

    private void Awake() {
        _gunController = GetComponent<GunController>();
        _chargeSound = SoundDatabase.Instance.GetSound(_chargeSoundName);
        _fullyChargedSound = SoundDatabase.Instance.GetSound(_fullyChargedSoundName);
    }

    private void OnEnable() {
        if (_gunController != null) {
            _gunController.StateMachine.StateChanged += OnGunStateChanged;
            _gunController.EnergyChanged += OnGunEnergyChanged;
            _gunController.SetActiveTriggered += OnSetActive;
        }
    }

    private void OnDisable() {
        if (_gunController != null) {
            _gunController.StateMachine.StateChanged -= OnGunStateChanged;
            _gunController.EnergyChanged -= OnGunEnergyChanged;
            _gunController.SetActiveTriggered -= OnSetActive;
        }
    }

    private void OnGunStateChanged(GunController.GunControllerState oldState,
        GunController.GunControllerState newState) {
        if (newState.StateIdentifier != GunController.GunControllerStateMachine.State.Idle)
            OnStopCharge();
    }

    public void OnGunEnergyChanged(float newEnergyValue) {
        _gunEnergyVisual.SetValue(newEnergyValue);

        CheckCharge(_lastEnergyValue, newEnergyValue);

        _lastEnergyValue = newEnergyValue;
    }

    // to disable sound if gun was disabled
    public void OnSetActive(bool setActive) {
        if (!setActive)
            OnStopCharge();
    }

    private void UpdateChargeSound(float currentChargeValue) {
        // set pitch of audio Source
        _source.pitch = _chargeSound.GetPitch(currentChargeValue);
    }

    private void CheckCharge(float oldValue, float newValue) {
        if (oldValue < newValue) {
            if (!_isCharging) {
                OnStartCharge(newValue);
            }

            UpdateChargeSound(newValue);

            if (newValue >= 1f) {
                OnStopCharge();
                OnFullCharged();
            }
        }
        else {
            if (_isCharging) {
                OnStopCharge();
            }
        }
    }

    private void OnStartCharge(float currentChargeValue) {
        _isCharging = true;

        // PlayChargeSound (looped)
        _chargeSound.InitSource(_source);
        _source.pitch = _chargeSound.GetPitch(currentChargeValue);
        _source.Play();
    }

    private void OnStopCharge() {
        _isCharging = false;

        // stopChargeSound
        _source.Stop();
    }

    private void OnFullCharged() {
        if (_source.isPlaying)
            _source.Stop();

        // play full charged sound
        _fullyChargedSound.InitSource(_source);
        _source.Play();
    }

    private void OnDestroy() {
        _gunEnergyVisual = null;
        if (_source != null) {
            if (_source.isPlaying)
                _source.Stop();

            Destroy(_source);
            _source = null;
        }

        _chargeSoundName = null;
        _chargeSound = null;

        _fullyChargedSoundName = null;
        _fullyChargedSound = null;
    }
}