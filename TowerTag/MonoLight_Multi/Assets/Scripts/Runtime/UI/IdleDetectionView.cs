using UI;
using UnityEngine;

[RequireComponent(typeof(IdleDetector))]
public class IdleDetectionView : MonoBehaviour {
    [SerializeField] private float _checkingInterval;
    [SerializeField] private float _timeUntilMessagePopup;
    [SerializeField] private float _timeUntilPlayerDisconnect;
    [SerializeField] private float _disconnectionTimeAdjustment;

    private IdleDetector _detector;

    private void Start() {
        _detector = GetComponent<IdleDetector>();
        _detector.StartIdleDetection(_checkingInterval, _timeUntilMessagePopup);
        _detector.OnIdleTimeExpired += ShowIdleWarning;
    }

    private void ShowIdleWarning() {
        MessageQueue.Singleton.AddButtonMessage(
            text:
            $"You have been inactive for {_timeUntilMessagePopup} seconds! You will be kicked in {_timeUntilPlayerDisconnect} seconds!",
            needsConfirmation: true,
            okButtonText: "I'm back",
            onOpen: OnOpen,
            onOkButton: CancelDisconnectionTimer,
            lifeTime: _timeUntilPlayerDisconnect + _disconnectionTimeAdjustment
        );
    }

    private void OnOpen() {
        Invoke(nameof(DisconnectPlayer), _timeUntilPlayerDisconnect);
    }

    private void CancelDisconnectionTimer() {
        CancelInvoke();
    }

    private void DisconnectPlayer() {
        ConnectionManager.Instance.LeaveRoom();
    }
}