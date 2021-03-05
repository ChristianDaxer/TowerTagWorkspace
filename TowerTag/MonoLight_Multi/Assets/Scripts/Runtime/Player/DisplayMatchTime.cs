using System;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMatchTime : MonoBehaviour {
    [SerializeField] private Text _text;
    private bool IsCurrentMatchRunning => GameManager.Instance.CurrentMatch != null && GameManager.Instance.CurrentMatch.MatchStarted;
    private void OnEnable() {
        if (!IsCurrentMatchRunning) {
            enabled = false;
        }
    }

    private void Update() {
        if (GameManager.Instance.MatchTimer == null) return;
        SetText(ConvertSecondsToMatchTimeString(GameManager.Instance.MatchTimer.GetCurrentTimerInSeconds()));
    }

    private string ConvertSecondsToMatchTimeString(float remainingSeconds) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(remainingSeconds));
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    private void SetText(string matchTime) {
        _text.text = matchTime;
    }
}
