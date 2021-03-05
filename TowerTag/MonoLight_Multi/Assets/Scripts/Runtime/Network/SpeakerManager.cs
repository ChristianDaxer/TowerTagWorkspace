using Network;
using Photon.Voice.Unity;
using TowerTag;
using UnityEngine;

public class SpeakerManager : MonoBehaviour
{
    private Player _player;
    [SerializeField] private Speaker _speaker;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        VoiceChatPlayer.Instance.ConversationGroupChanged += OnConversationGroupChanged;
    }

    private void OnDisable()
    {
        if(VoiceChatPlayer.Instance != null)
            VoiceChatPlayer.Instance.ConversationGroupChanged -= OnConversationGroupChanged;
    }

    private void OnConversationGroupChanged(VoiceChatPlayer.ChatType chatType)
    {
        if (chatType == VoiceChatPlayer.ChatType.TalkInTeam)
        {
            if (_player.IsMe) return;
            IPlayer localPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (localPlayer != null && localPlayer.TeamID != _player.TeamID)
                _speaker.enabled = false;
        }
        else
        {
            _speaker.enabled = true;
        }
    }

    private void ToggleSpeaker(bool status)
    {
        if(_player.IsMe) return;
        if (_speaker == null)
        {
            Debug.LogError("Can't find Players Speaker Component.");
            return;
        }

        _speaker.enabled = status;
    }

    public void ActivatePlayerSpeaker()
    {
        ToggleSpeaker(true);
    }

    public void DeactivatePlayerSpeaker()
    {
        ToggleSpeaker(false);
    }
}
