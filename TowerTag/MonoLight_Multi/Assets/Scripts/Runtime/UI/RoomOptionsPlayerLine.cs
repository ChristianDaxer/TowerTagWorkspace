using Home.UI;
using JetBrains.Annotations;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class RoomOptionsPlayerLine : MonoBehaviour
{
    public enum RoomOptionAction
    {
        Mute,
        Kick,
        Report
    }
    
    public delegate void RoomOptionPlayerLineAction(object sender, IPlayer player, RoomOptionAction buttonAction, bool buttonStatus);

    public event RoomOptionPlayerLineAction SomeRoomOptionButtonPressed;

    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private Button _muteButton;
    [SerializeField] private Button _kickButton;
    [SerializeField] private Animator _muteButtonAnimator;
    [SerializeField] private Image _background;

    [SerializeField] private Player _ownPlayerTestField;
    
    private IPlayer _owner;

    public IPlayer Owner => _owner;

    private RoomLine.BackgroundStyle _backgroundStyle;
    private bool _muteActive;
    private static readonly int Active = Animator.StringToHash("Active");

    public RoomOptionsPlayerLine Create(Transform parent, IPlayer player)
    {
        if (player == null) {
            Debug.LogError("PlayerLines referenced Player is null.");
            return null;
        }
        _owner = player;
        _ownPlayerTestField = _owner as Player;
        _playerName.text = _owner.PlayerName;

        RoomOptionsPlayerLine playerLine = InstantiateWrapper.InstantiateWithMessage(this, parent);
        playerLine.SetBackgroundStyle(RoomLine.BackgroundStyle.Light);
        return playerLine;
    }

    private void ResetRoomOptionsPlayerLine()
    {
        _muteActive = false;
        _playerName.text = "ANONYMOUS";
    }

    [UsedImplicitly]
    public void OnMuteButtonPressed()
    {
        if (_owner == null)
        {
            //Debug.LogError("PlayerLines referenced Player is null.");
            
            // q & d fix for owner == null
            _owner = _ownPlayerTestField;
            //return;
        }
        _muteActive = !_muteActive;
        SetAnimatorState(_muteButtonAnimator, _muteActive);
        SomeRoomOptionButtonPressed?.Invoke(this, _owner, RoomOptionAction.Mute, _muteActive);
    }
    
    [ContextMenu("Mute Button Press Test")]
    public void OnMuteButtonPressTest()
    {
        if (_owner == null)
        {
            Debug.LogError("PlayerLines referenced Player is null.");
            return;
        }
        _muteActive = !_muteActive;
        SetAnimatorState(_muteButtonAnimator, _muteActive);
        SomeRoomOptionButtonPressed?.Invoke(this, _owner, RoomOptionAction.Mute, _muteActive);
    }

    [UsedImplicitly]
    public void OnKickButtonPressed()
    {
        if (_owner == null)
        {
            return;
        }
        _muteActive = !_muteActive;
        SetAnimatorState(_muteButtonAnimator, _muteActive);
        SomeRoomOptionButtonPressed?.Invoke(this, _owner, RoomOptionAction.Kick, false);
    }

    [UsedImplicitly]
    public void OnReportButtonPressed()
    {
        SomeRoomOptionButtonPressed?.Invoke(this, _owner, RoomOptionAction.Report, false);
    }

    public void SetBackgroundStyle(RoomLine.BackgroundStyle backgroundStyle)
    {
        _backgroundStyle = backgroundStyle;
        Refresh();
    }

    private void Refresh()
    {
        Color baseColor = TeamManager.Singleton.TeamIce.Colors.UI;

        if (_backgroundStyle == RoomLine.BackgroundStyle.Dark)
        {
            _background.color = baseColor * 0.0f;
            _playerName.color = TeamManager.Singleton.TeamIce.Colors.UI;
        }

        if (_backgroundStyle == RoomLine.BackgroundStyle.Light)
        {
            _background.color = baseColor * 0.3f;
            _playerName.color = TeamManager.Singleton.TeamIce.Colors.UI;
        }
    }

    private void SetAnimatorState(Animator animator, bool state)
    {
        animator.SetBool(Active, state);
    }

    public void SetButtonsStates(bool muteButtonState)
    {
        if (_muteActive == muteButtonState) return;

        _muteActive = muteButtonState;
        SetAnimatorState(_muteButtonAnimator, _muteActive);
    }
}