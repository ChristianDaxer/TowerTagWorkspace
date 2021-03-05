using Photon.Pun;
using UnityEngine;

public static class QueueTimerManager {
    public static float HubSceneTime { get; private set; }
    public static float RestWaitingTime { get; private set; }
    public static bool Autostart { get; private set; }

    private const float _waitingTime = 120;
    private const float _countdownTime = 10;
    private static bool _expired => RestWaitingTime <= 0f;

    public static float GetTime() {
        return Autostart ? RestWaitingTime : HubSceneTime;
    }

    public static void OverrideTimer(float time) {
        RestWaitingTime = time;
    }

    public static void ResetAndStartQueueTimer() {
        StartQueueTimer();
        ResetValues();
    }

    public static void StartQueueTimer() {
        if (!GameManager.Instance.TrainingVsAI && PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.Autostart)) {
            Autostart = (bool) PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.Autostart];
        }
    }

    public static void StopQueueTimer() {
        Autostart = false;
        ResetValues();
    }

    public static void Tick() {
        if (Autostart) {
            if (PhotonNetwork.IsMasterClient) {
                if (_expired) {
                    if (GameManager.Instance.MatchCountdownRunning) return;
                    StartCountdown();
                }
                else {
                    RestWaitingTime -= Time.deltaTime;
                }
            }
            else {
                if (!_expired) RestWaitingTime -= Time.deltaTime;
            }
        }
        else {
            if (!GameManager.Instance.MatchCountdownRunning)
                HubSceneTime += Time.deltaTime;
        }
    }

    private static void StartCountdown() {
        if (GameManager.Instance.TrainingVsAI) return;

        GameManager.Instance.StartMatchCountdown(_countdownTime);
    }

    private static void ResetValues() {
        HubSceneTime = 0;
        RestWaitingTime = _waitingTime;
        PlayerManager.Instance.GetOwnPlayer()?.ResetButtonStates();
    }
}