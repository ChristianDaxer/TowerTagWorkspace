using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MatchInfoText : MonoBehaviour {
    [SerializeField] private Text _text1;
    [SerializeField] private string _waitingForOtherPlayers = "WAITING FOR OTHERS";
    [SerializeField] private string _waitingForStartVote = "WAITING FOR STARTVOTES";
    [SerializeField] private string _waitingForOtherPlayersWithQueue = "%r | %t";
    [SerializeField] private string _matchStartsIn = "MATCH STARTS IN %s S";
    [SerializeField] private string _matchStarting = "MATCH STARTING...";

    private Coroutine _timerCoroutine;

    private void OnEnable() {
        GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
        if (GameManager.Instance.MatchCountdownRunning) {
            OnBasicCountdownStarted(GameManager.Instance.TrainingVsAI
                ? TowerTagSettings.TrainingModeStartMatchCountdownTime
                : TowerTagSettings.BasicModeStartMatchCountdownTime);
        }
        else if (TowerTagSettings.Home) {
            if (_timerCoroutine == null)
                _timerCoroutine = StartCoroutine(UpdateTimer());
        }
    }

    private void OnDisable() {
        GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted -= OnBasicCountdownAborted;
    }

    private void OnBasicCountdownAborted() {
        try {
            StopAllCoroutines();
            _text1.text = _waitingForOtherPlayers;
            if (TowerTagSettings.Home) {
                _timerCoroutine = StartCoroutine(UpdateTimer());
                _timerCoroutine = null;
            }
        }
        catch (Exception e) {
            Debug.LogError($"Failed to update text: {e}");
        }
    }

    private void OnBasicCountdownStarted(float countdownTime) {
        try {
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);
            StartCoroutine(CountdownCoroutine(countdownTime));
        }
        catch (Exception e) {
            Debug.LogError($"Failed to update text: {e}");
        }
    }

    private IEnumerator CountdownCoroutine(float countdownTime) {
        float t = countdownTime;
        while (t > 0) {
            yield return null;
            t -= Time.deltaTime;
            string text = _matchStartsIn.Replace("%s", Mathf.Ceil(t).ToString(CultureInfo.InvariantCulture));
            _text1.text = text;
        }

        _text1.text = _matchStarting;
    }

    private IEnumerator UpdateTimer() {
        if (!TowerTagSettings.Home) yield break;
        while (true) {
            TimeSpan t = TimeSpan.FromSeconds(QueueTimerManager.Autostart ? QueueTimerManager.RestWaitingTime : QueueTimerManager.HubSceneTime);

            string answer = $"{t.Minutes:D2}:{t.Seconds:D2}";
            _text1.text = _waitingForOtherPlayersWithQueue.Replace("%t", answer)
                .Replace("%r", GameManager.Instance.CurrentHomeMatchType != GameManager.HomeMatchType.Custom
                    ? _waitingForOtherPlayers
                    : _waitingForStartVote);
            yield return null;
        }
    }
}