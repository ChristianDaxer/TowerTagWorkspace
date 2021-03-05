using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ReadyTowerUIPUNCallback : MonoBehaviourPunCallbacks {
    public delegate void ReadyTowerUIDependentPropertyAction(bool status);

    public delegate void ReadyTowerUIDependentPropertyGameModeAction(GameMode currentGameMode);

    public static event ReadyTowerUIDependentPropertyAction UserVoteToggled;
    public static event ReadyTowerUIDependentPropertyAction AllowTeamChangeToggled;
    public static event ReadyTowerUIDependentPropertyGameModeAction NewCurrentGameModeSelected;

    private static bool _userVote;
    private static bool _allowTeamChange;

    public static bool AllowTeamChange => _allowTeamChange;

    public static bool UserVote => _userVote;

    private GameMode _currentGameMode;

    private void Awake() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.GameMode))
            NewCurrentGameModeSelected?.Invoke(_currentGameMode);
    }

    public void GetInitialRoomProperties() {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (roomProperties.ContainsKey(RoomPropertyKeys.GameMode))
            _currentGameMode = (GameMode) roomProperties[RoomPropertyKeys.GameMode];
        if (roomProperties.ContainsKey(RoomPropertyKeys.UserVote))
            _userVote = (bool) roomProperties[RoomPropertyKeys.UserVote];
        if (roomProperties.ContainsKey(RoomPropertyKeys.AllowTeamChange))
            _allowTeamChange = (bool) roomProperties[RoomPropertyKeys.AllowTeamChange];
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey("UV") &&
            (bool) propertiesThatChanged[RoomPropertyKeys.UserVote] != UserVote) {
            _userVote = !UserVote;
            UserVoteToggled?.Invoke(UserVote);
        }

        if (propertiesThatChanged.ContainsKey("ATC") &&
            (bool) propertiesThatChanged[RoomPropertyKeys.AllowTeamChange] != AllowTeamChange) {
            _allowTeamChange = !AllowTeamChange;
            AllowTeamChangeToggled?.Invoke(AllowTeamChange);
        }

        if (propertiesThatChanged.ContainsKey(RoomPropertyKeys.GameMode) &&
            (GameMode) propertiesThatChanged[RoomPropertyKeys.GameMode] != _currentGameMode) {
            _currentGameMode = (GameMode) propertiesThatChanged[RoomPropertyKeys.GameMode];
            NewCurrentGameModeSelected?.Invoke(_currentGameMode);
        }
    }
}