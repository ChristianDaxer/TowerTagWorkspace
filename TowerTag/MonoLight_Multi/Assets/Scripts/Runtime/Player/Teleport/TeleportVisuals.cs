using UnityEngine;

[RequireComponent(typeof(TeleportMovement))]
public class TeleportVisuals : MonoBehaviour
{
    [SerializeField]
    private GameObject _effectsParent;

    [SerializeField] private AudioSource _teleportSoundsSource;

    [SerializeField] private string _teleportStartSoundName;
    private Sound _teleportStartSound;

    private TeleportMovement _teleportMovement;

    protected virtual void Awake() {
        _teleportMovement = GetComponent<TeleportMovement>();
        _teleportStartSound = SoundDatabase.Instance.GetSound(_teleportStartSoundName);
    }

    private void OnEnable() {
        if (_teleportMovement != null) {
            _teleportMovement.TeleportStarted += OnTeleportStarted;
            _teleportMovement.Teleported += OnTeleportFinished;
        }
        else {
            Debug.LogError("Could not find TeleportMovement Component. Will not visualize teleports.");
        }
    }

    private void OnDisable() {
        if (_teleportMovement != null) {
            _teleportMovement.TeleportStarted -= OnTeleportStarted;
            _teleportMovement.Teleported -= OnTeleportFinished;
        }
    }

    private void OnTeleportStarted(int newPillarID, Transform rootTransform)
    {
        ActivateEffects(true);

        TriggerStartTeleportSound();
    }

    private void OnTeleportFinished (Transform rootTransform)
    {
        ActivateEffects(false);

        //TriggerTeleportFinishedSound();
    }

    private void TriggerStartTeleportSound()
    {
        if (_teleportSoundsSource == null) {
            Debug.LogWarning("Cannot initialize teleport start sound. Source is null");
            return;
        }
        if (_teleportStartSound == null) {
            Debug.LogWarning("Cannot play teleport start sound. Sound is null");
            return;
        }

        if (_teleportSoundsSource.isPlaying)
            _teleportSoundsSource.Stop();

        _teleportStartSound.InitSource(_teleportSoundsSource);
        _teleportSoundsSource.Play();
    }

    protected virtual void ActivateEffects(bool setActive)
    {
        if (_effectsParent != null)
        {
            _effectsParent.SetActive(setActive);
        }
    }
}
