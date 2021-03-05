using System.Collections;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class DisabledGunControllerVisuals : MonoBehaviour {
    public delegate void CountdownTimeHandler();

    public event CountdownTimeHandler DisplayedCountdownTimeChanged;
    public event CountdownTimeHandler CountdownFinished;

    [SerializeField] private IPlayer _player;
    [SerializeField] private Text _countdownTextField;
    [SerializeField] private string _countdownText = "You 're back in the game in \n";

    private MatchTimer _timer;
    private int _secondsLeft;
    private bool _wasInCountDownLastFrame;
    private Coroutine _currentCountdownTimer;

    private IPlayer Player {
        get => _player;
        set {
            UnregisterEventListeners();
            _player = value;
            RegisterEventListeners();
        }
    }

    private void RegisterEventListeners() {
        if (Player != null) {
            Player.CountdownStarted += ActivateCountdown;
        }
    }

    private void UnregisterEventListeners() {
        if (Player != null)
            Player.CountdownStarted -= ActivateCountdown;
    }

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void Start() {
        Player = GetComponentInParent<IPlayer>();
    }

    /// <summary>
    /// When the duration is > 0 it will start a Countdown after a delay in duration's length
    /// </summary>
    private void ActivateCountdown(int startAtTime, int countdownTypeInt) {
        var countdownType = (Match.CountdownType) countdownTypeInt;
        var countdownDuration = 0;

        if (countdownType == Match.CountdownType.StartMatch
            || countdownType == Match.CountdownType.ResumeMatch)
            countdownDuration = GameManager.Instance.MatchStartCountdownTimeInSec;
        else if (countdownType == Match.CountdownType.StartRound)
            countdownDuration = GameManager.Instance.RoundStartCountdownTimeInSec;

        // deactivate player -> activate limbo (saturation = minSaturation)
        int timestampToStartCountdown = startAtTime - (countdownDuration * 1000);
        ShowCountdownTextField(true);

        // show reactivation countdown timer to player
        if (_currentCountdownTimer != null) {
            StopCoroutine(_currentCountdownTimer);
        }

        _currentCountdownTimer =
            StartCoroutine(ShowReactivationCountdown(countdownDuration, timestampToStartCountdown));
    }


    /// <summary>
    /// Show reactivation countdown timer to player (on VR Controller).
    /// </summary>
    /// <param name="duration">Countdown length.</param>
    /// <param name="timestampToStartCountdown">Time to start countdown</param>
    /// <returns></returns>
    private IEnumerator ShowReactivationCountdown(int duration, int timestampToStartCountdown) {
        if (_countdownTextField == null)
            yield break;

        if (duration <= 0)
            yield break;

        _countdownTextField.text = _countdownText + duration + " s.";
        while (timestampToStartCountdown >= PhotonNetwork.ServerTimestamp) {
            yield return null;
        }

        float startTime = Time.realtimeSinceStartup;
        int countDownTimeInSeconds = duration;
        int currentlyShownSecondsLeft = 0;

        while (countDownTimeInSeconds >= 0) {
            countDownTimeInSeconds = Mathf.CeilToInt(duration - (Time.realtimeSinceStartup - startTime));
            if (Mathf.Max(countDownTimeInSeconds, 0) == 0) {
                ShowCountdownTextField(false);
                CountdownFinished?.Invoke();
                yield break;
            }

            if (currentlyShownSecondsLeft != Mathf.Max(countDownTimeInSeconds, 0)) {
                currentlyShownSecondsLeft = Mathf.Max(countDownTimeInSeconds, 0);
                _countdownTextField.text = _countdownText + currentlyShownSecondsLeft + " s.";
                DisplayedCountdownTimeChanged?.Invoke();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Enable/disable Textfield to show countdown on.
    /// </summary>
    /// <param name="show"></param>
    private void ShowCountdownTextField(bool show) {
        if (_countdownTextField != null) {
            _countdownTextField.text = "";

            _countdownTextField.enabled = show;
        }
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    private void OnDestroy() {
        if (_currentCountdownTimer != null) {
            StopCoroutine(_currentCountdownTimer);
            _currentCountdownTimer = null;
        }

        _countdownTextField = null;
        _countdownText = null;
    }
}