using System.Collections;
using System.Linq;
using SOEventSystem.Shared;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.EventSystems;

public class TeamBoxController : MonoBehaviour {
    [SerializeField, Header("General")] private string _defaultTeamName;
    [SerializeField] private TeamID _teamID;
    [SerializeField] private AdminPlayerLineSlot[] _slots;
    [SerializeField] private SharedBool _editingTeamName;

    [SerializeField, Header("UI Objects")] private TMP_InputField _teamNameInputField;
    [SerializeField] private AddBotButton _addBotButton;

    [SerializeField] private PlayerLineController _playerLinePrefab;
    [SerializeField] private PlayerLineController _playerLineBotPrefab;
    private bool _cellsEnabled = true;

    public TeamID TeamID => _teamID;
    private ITeam Team => TeamManager.Singleton.Get(_teamID);

    private void Start() {
        _editingTeamName.Set(this, false);
    }

    private void OnEnable() {
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        DragAndDropItem.OnItemDragStartEvent += OnDragStarted;
        DragAndDropItem.OnItemDragEndEvent += OnDragEnded;
        GameManager.Instance.MatchConfigurationStarted += OnConfigurationStarted;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
    }

    private void OnDisable() {
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        DragAndDropItem.OnItemDragStartEvent -= OnDragStarted;
        DragAndDropItem.OnItemDragEndEvent -= OnDragEnded;
        GameManager.Instance.MatchConfigurationStarted -= OnConfigurationStarted;
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
        StartCoroutine(ToggleAddButtonIfNeeded());
    }

    private void OnConfigurationStarted() {
        StartCoroutine(ToggleAddButtonIfNeeded());
    }

    private void OnDragStarted(DragAndDropItem item) {
        if (!_cellsEnabled) return;
        if (item.GetComponentInChildren<PlayerLineController>()?.Player.TeamID == _teamID) return;
        AdminPlayerLineSlot freeSlot = GetFirstFreeSlot();
        if (freeSlot != null) freeSlot.SetVisible(true);
    }

    private void OnDragEnded(DragAndDropItem item) {
        ShowFilledSlots();
        StartCoroutine(ToggleAddButtonIfNeeded());
    }

    public void HideSlots() {
        foreach (AdminPlayerLineSlot slot in _slots) {
            slot.SetVisible(false);
        }
    }

    public void ShowFilledSlots() {
        foreach (AdminPlayerLineSlot slot in _slots) {
            slot.SetVisible(!slot.IsFree);
        }
    }

    /// <summary>
    /// Adds a PlayerLine for a specific Player
    /// </summary>
    /// <param name="player">The Player whose PlayerLine should be added</param>
    private void OnPlayerAdded(IPlayer player) {
        if (player.TeamID != _teamID) return;
        // Instantiate the PlayerLine and set it as a child

        PlayerLineController newPlayerLine =
            InstantiateWrapper.InstantiateWithMessage(player.IsBot ? _playerLineBotPrefab : _playerLinePrefab, transform);

        // Set the player
        newPlayerLine.Player = player;
        newPlayerLine.UpdateTeamColor(player, player.TeamID);

        // Set normal player text material
        newPlayerLine.TalkTo = false;
        newPlayerLine.Focus = false;

        // Add the Player line to the team box
        AddPlayerLine(newPlayerLine);
        StartCoroutine(ToggleAddButtonIfNeeded());
    }

    private void OnPlayerRemoved(IPlayer player) {
        if (player.TeamID != _teamID) return;
        foreach (AdminPlayerLineSlot slot in _slots) {
            if (slot.Player == player) slot.Clear();
        }

        StartCoroutine(ToggleAddButtonIfNeeded());
    }

    public void OnInputFieldSelect() {
        _editingTeamName.Set(this, true);
    }

    public void OnInputFieldEndEdit() {
        if (string.IsNullOrEmpty(_teamNameInputField.text)) {
            _teamNameInputField.text = _defaultTeamName;
            _teamNameInputField.OnControlClick();
        }

        SetTeamName(_teamNameInputField.text);
        _editingTeamName.Set(this, false);

        //If the input field is still selected we have to force to free this
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Sets all the DragAndDropCells of the TeamBoxController to a new CellType
    /// </summary>
    /// <param name="newCellType"></param>
    private void SetCellTypeOfSlots(DragAndDropCell.CellType newCellType) {
        foreach (AdminPlayerLineSlot slot in _slots) {
            slot.SetCellType(newCellType);
        }
    }

    /// <summary>
    /// Set the Team Name
    /// </summary>
    /// <param name="newTeamName">The new team name</param>
    public void SetTeamName(string newTeamName) {
        _teamNameInputField.text = newTeamName;
        Team.SetName(_teamNameInputField.text);
    }

    private void AddPlayerLine(PlayerLineController playerLine) {
        AdminPlayerLineSlot slot = GetFirstFreeSlot();

        if (slot != null) {
            slot.Fill(playerLine.gameObject);

            //during the game, the cell should not be swappable!
            if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure)
                slot.SetCellType(DragAndDropCell.CellType.Swap);
            else {
                slot.SetCellType(DragAndDropCell.CellType.DropOnly);
                playerLine.ToggleMenuButton(false);
            }
        }
        else {
            Destroy(playerLine.gameObject);
            Debug.LogWarning("Couldn't add player " + playerLine.name + " because there was no free slot in the " +
                             Team.Name + " Team");
        }
    }

    /// <summary>
    /// Checks the condition if the AddBotButton has to be toggled in this TeamBoxController and toggles it
    /// </summary>
    private IEnumerator ToggleAddButtonIfNeeded() {
        if (GameManager.Instance.CurrentState != GameManager.GameManagerStateMachine.State.Configure) {
            _addBotButton.Close();
            yield break;
        }
        yield return new WaitForEndOfFrame();
        if (GetFirstFreeSlot() == null)
            _addBotButton.Close();
        else
            _addBotButton.Open();
    }

    /// <summary>
    /// Iterates through all DragAndDropCells of the TeamBoxController
    /// </summary>
    /// <returns>A DragAndDropCell if there is a free one, else null</returns>
    public AdminPlayerLineSlot GetFirstFreeSlot() {
        return _slots.OrderBy(slot => slot.transform.GetSiblingIndex()).FirstOrDefault(slot => slot.IsFree);
    }

    public PlayerLineController GetPlayerLine(IPlayer player) {
        foreach (AdminPlayerLineSlot slot in _slots) {
            if (slot.Player == player) return slot.GetComponentInChildren<PlayerLineController>();
        }

        return null;
    }

    /// <summary>
    /// Enables or disables all DragAndDropCells on the Admin Panel
    /// </summary>
    /// <param name="cellsEnabled">Should the DragAndDrop Cells be enabled?</param>
    public void SetDragAndDropCellsEnabled(bool cellsEnabled) {
        _cellsEnabled = cellsEnabled;
        if (_cellsEnabled) {
            SetCellTypeOfSlots(DragAndDropCell.CellType.Swap);
        }
        else {
            GetComponentsInChildren<DragAndDropCell>().ForEach(slot => slot.GetItem()?.OnEndDrag(null));
            SetCellTypeOfSlots(DragAndDropCell.CellType.DropOnly);
        }
    }
}