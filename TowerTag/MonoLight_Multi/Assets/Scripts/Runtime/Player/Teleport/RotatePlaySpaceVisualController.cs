using UnityEngine;

public class RotatePlaySpaceVisualController : MonoBehaviour {
    [SerializeField] private AudioSource _teleportSoundsSource;

    [SerializeField] private string _teleportStartSoundName;
    private Sound _teleportStartSound;
    private bool _missingReferences;
    private bool _initialized;

    private RotatePlaySpaceMovement _rotationMovement;

    private void Awake() {
        _rotationMovement = GetComponent<RotatePlaySpaceMovement>();
        _teleportStartSound = SoundDatabase.Instance.GetSound(_teleportStartSoundName);
    }

    private void OnEnable() {
        if (!_initialized) Init();
    }

    private void OnDisable() {
        DeregisterListener();
    }

    private void Init() {
        _missingReferences = false;

        if (_teleportSoundsSource == null) {
            Debug.LogError("Could not find teleport sound source. Will not visualize play space rotation.");
            _missingReferences = true;
        }

        if (_teleportStartSound == null) {
            Debug.LogError("Could not find teleport sound. Will not visualize play space rotation.");
            _missingReferences = true;
        }

        if (_teleportStartSoundName == null) {
            Debug.LogError("Could not find teleport sound name. Will not visualize play space rotation.");
            _missingReferences = true;
        }

        if (_rotationMovement == null) {
            Debug.LogError("Could not find RotationMovement Component. Will not visualize play space rotation.");
            _missingReferences = true;
        }

        if (_missingReferences) return;

        _initialized = true;
        RegisterListener();
    }

    private void RegisterListener() {
        if (_missingReferences) return;

        _rotationMovement.RotationStarted += OnRotationStarted;
        _rotationMovement.Rotated += OnRotationFinished;
    }

    private void DeregisterListener() {
        if (_missingReferences) return;
        _rotationMovement.RotationStarted -= OnRotationStarted;
        _rotationMovement.Rotated -= OnRotationFinished;
    }

    private void OnRotationStarted(object sender, RotatePlaySpaceMovement.PlaySpaceDirection turningPlateDirection) {
        if (_missingReferences) return;

        TriggerRotationSound();
        ToggleSomeOtherVisuals(true);
    }

    private void OnRotationFinished(object sender, RotatePlaySpaceMovement.PlaySpaceDirection turningPlateDirection) {
        if (_missingReferences) return;

        ToggleSomeOtherVisuals(false);
    }

    private void TriggerRotationSound() {
        if (_missingReferences) return;

        if (_teleportSoundsSource.isPlaying)
            _teleportSoundsSource.Stop();

        _teleportStartSound.InitSource(_teleportSoundsSource);
        _teleportSoundsSource.Play();
    }

    private void ToggleSomeOtherVisuals(bool status) {
        // TODO Placeholder for some other Visuals
    }
}