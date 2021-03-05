using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeToTTSoundTrigger : MonoBehaviour {
    [SerializeField] private bool _playOnlyWhenFirstTimeInHubScene = true;
    [SerializeField] private AudioSource _source;
    [SerializeField] private string _welcomeSoundName = "56_WelcomeToTT";
    [SerializeField] private float _playSoundDelay = 3f;

    private bool _playedWelcomeAlready;
    private Sound _welcomeSound;
    private Coroutine _playWelcomeCoroutine;

    private void Start() {
        SceneManager.sceneLoaded -= NewSceneWasLoaded;
        SceneManager.sceneLoaded += NewSceneWasLoaded;
        _playedWelcomeAlready = false;

        InitSound();
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= NewSceneWasLoaded;
    }

    private void NewSceneWasLoaded(Scene scene, LoadSceneMode mode) {
        if (SharedControllerType.IsAdmin)
            return;

        if (TTSceneManager.Instance == null)
            return;

        if (!TTSceneManager.Instance.IsInHubScene) {
            if (_source != null)
                _source.Stop();

            return;
        }

        if (!_playedWelcomeAlready || !_playOnlyWhenFirstTimeInHubScene) {
            if (_playWelcomeCoroutine != null)
                StopCoroutine(_playWelcomeCoroutine);

            _playWelcomeCoroutine = StartCoroutine(HelperFunctions.Wait(_playSoundDelay, PlayWelcomeSound));
        }
    }

    private void InitSound() {
        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot initialize sound: no sound database found");
            return;
        }

        _welcomeSound = SoundDatabase.Instance.GetSound(_welcomeSoundName);

        if (_welcomeSound == null) {
            Debug.LogError("Cannot initialize sound: sound not found in database");
            return;
        }

        if (_source == null) {
            Debug.LogError("Cannot initialize sound: no audio source");
            return;
        }

        _welcomeSound.InitSource(_source);
    }

    private void PlayWelcomeSound() {
        _playedWelcomeAlready = true;

        if (_source == null) {
            Debug.LogError("Cannot play welcome sound: no audio source");
            return;
        }

        _source.Play();
    }
}