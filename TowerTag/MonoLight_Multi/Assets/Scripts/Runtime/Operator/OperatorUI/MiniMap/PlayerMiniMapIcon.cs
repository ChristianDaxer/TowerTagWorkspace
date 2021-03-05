using OperatorCamera;
using TMPro;
using TowerTag;
using UnityEngine;

public sealed class PlayerMiniMapIcon : MiniMapIcon {
    [SerializeField, Tooltip("The mini map sprite for the player when he is not focused by the cam")]
    private Sprite _unfocusedPlayer;
    [SerializeField, Tooltip("The mini map sprite for the player when he is focused by the cam")]
    private Sprite _focusedPlayer;
    [field: SerializeField, Tooltip("The text field for the player number")]
    public TMP_Text PlayerNumber { get; set; }

    private bool _playerFocused;
    //The player number euler angles! Have to keep them on team change and late join!
    private readonly Vector3 _rotationVector = new Vector3(90,-90,0);

    private void Start() {
        CameraManager.Instance.PlayerToFocusChanged += OnPlayerFocusChanged;
        //Rotate around 180 to display the number correct
        if (Player != null) {
            Player.PlayerTeamChanged += OnPlayerTeamChanged;
            Player.ParticipatingStatusChanged += OnParticipatingStatusChanged;
            OnParticipatingStatusChanged(Player, Player.IsParticipating);
            OnPlayerTeamChanged(Player, Player.TeamID);
        }
    }

    private void OnPlayerTeamChanged(IPlayer player, TeamID obj) {
        PaintIcons(GameManager.Instance.CurrentMatch);
    }

    private void OnParticipatingStatusChanged(IPlayer player, bool newValue) {
        gameObject.SetActive(newValue);
    }

    private void OnPlayerFocusChanged(CameraManager sender, IPlayer player) {
        if (_controlledImage == null) return;
        if (player == Player) {
            _controlledImage.sprite = _focusedPlayer;
            PlayerNumber.color = TeamManager.Singleton.Get(Player.TeamID).Colors.DarkUI;
            _playerFocused = true;
        }
        else {
            if (!_playerFocused) return;
            _controlledImage.sprite = _unfocusedPlayer;
            PlayerNumber.color = TeamManager.Singleton.Get(Player.TeamID).Colors.UI;
            _playerFocused = false;
        }
    }

    private new void Update () {
        base.Update();
        PlayerNumber.transform.eulerAngles = _rotationVector;
    }

    /// <summary>
    /// Every Start of a match the team colors should be updated
    /// </summary>
    /// <param name="obj"></param>
    protected override void PaintIcons(IMatch obj)
    {
        base.PaintIcons(obj);
        PlayerNumber.color = _playerFocused
                ? TeamManager.Singleton.Get(Player.TeamID).Colors.DarkUI
                : TeamManager.Singleton.Get(Player.TeamID).Colors.UI;
    }

    private void OnDestroy() {
        if (CameraManager.Instance != null)
            CameraManager.Instance.PlayerToFocusChanged -= OnPlayerFocusChanged;
        if (Player != null) {
            Player.PlayerTeamChanged -= OnPlayerTeamChanged;
            Player.ParticipatingStatusChanged -= OnParticipatingStatusChanged;
        }
    }
}
