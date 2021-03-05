using JetBrains.Annotations;
using Network;
using UnityEngine;

public class OperatorVoice : MonoBehaviour {
    [UsedImplicitly]
    public void ActivateAdminVoice() {
        VoiceChatPlayer.Instance.TogglePushToTalkOnOff(true);
    }

    [UsedImplicitly]
    public void DeactivateAdminVoice() {
        VoiceChatPlayer.Instance.TogglePushToTalkOnOff(false);
    }
}
