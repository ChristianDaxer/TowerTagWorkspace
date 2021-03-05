using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ReadyTowerUiGameModeButton : MonoBehaviour {
    public delegate void GameModeButtonAction(ReadyTowerUiGameModeButton sender, GameMode selectedMode);

    public event GameModeButtonAction GameModeSelected;

    [SerializeField] private Button _gameModeButton;
    [SerializeField] private GameMode _gameMode;
    [SerializeField] private Sprite _gameModeImage;
    [SerializeField] private Color _disabledImageColor;
    [SerializeField] private GameObject _selectionLine;
    [SerializeField] private Text _textGameMode;
    [SerializeField] private GameObject _voteLine;
    [SerializeField] private GameObject _votePrefab;
    [SerializeField] private GameObject _modeLayerGroup;
    [SerializeField] private float _modeLayerGroupZOffset = 50f;
    [SerializeField] private Text _modeInfoOverlayText;
    [SerializeField] private Material _hoverMaterial;
    [SerializeField] private Material _votedMaterial;
    private Renderer[] _selectionLines;
    private Vector3 _startPosition;
    private Vector3 _hoverAndActivePosition;
    private bool _positionInitialized;

    private bool Active { get; set; }
    public int VoteCount => _voteLine.transform.childCount;

    public GameMode Mode => _gameMode;

    public void Init(bool status) {
        if (_textGameMode != null)
            _textGameMode.text = Mode.ToString().ToUpper();

        if (_selectionLine != null) {
            _selectionLine.SetActive(!status);
            _selectionLines = _selectionLine.GetComponentsInChildren<Renderer>();
            _selectionLines.ForEach(rend => rend.material = _hoverMaterial);
        }

        // reset Button
        if (_gameModeButton != null) {
            _gameModeButton.interactable = status;
            _gameModeButton.image.sprite = _gameModeImage;
        }

        // disable mode info text
        if (_modeInfoOverlayText != null)
            ToggleModeInfoOverlayText(false, "none");

        Active = status;
    }

    public void SelectButtonManually(bool setActive) {
        _selectionLine.SetActive(setActive);
        _selectionLines.ForEach(rend => rend.material = setActive ? _votedMaterial : _hoverMaterial);
        _gameModeButton.image.color = setActive ? Color.white : _disabledImageColor;
    }

    public void ToggleModeButtonImageColor(bool active) {
        _gameModeButton.image.color = active ? Color.white : _disabledImageColor;
    }

    [UsedImplicitly]
    public void OnClickGameModeButton() {
        _selectionLine.SetActive(true);
        _selectionLines.ForEach(rend => rend.material = _votedMaterial);
        GameModeSelected?.Invoke(this, Mode);
    }

    [UsedImplicitly]
    public void OnPointerEnter() {
        if (_selectionLine.activeSelf || !Active)
            return;
        if(!_positionInitialized)
            InitPositionValues();
        _modeLayerGroup.transform.localPosition = _hoverAndActivePosition;
        _selectionLine.SetActive(true);
    }

    private void InitPositionValues() {
        _startPosition = _modeLayerGroup.transform.localPosition;
        _hoverAndActivePosition = _modeLayerGroup.transform.localPosition + new Vector3(0, 0, _modeLayerGroupZOffset);
        _positionInitialized = true;
    }

    [UsedImplicitly]
    public void OnPointerExit() {
        if (!Active)
            return;
        _modeLayerGroup.transform.localPosition = _startPosition;
        _selectionLine.SetActive(false);
    }

    public void ToggleGameModeButton(bool status, bool forceToggle = false) {
        if (!forceToggle) {
            if(Active && !status)
                _modeLayerGroup.transform.localPosition = _hoverAndActivePosition;
            if (IsGameModePlayable()) {
                _gameModeButton.interactable = status;
                Active = status;
            }
            else {
                switch (Mode) {
                    case GameMode.GoalTower:
                        ToggleModeInfoOverlayText(true, "> NOT ENOUGH PLAYERS <");
                        break;
                }

                _gameModeButton.interactable = false;
                Active = false;
            }
        }
        else
        {
            _gameModeButton.interactable = status;
            Active = status;
        }


        if (PlayerManager.Instance.GetOwnPlayer()?.VoteGameMode != _gameMode)
            _gameModeButton.image.color = _gameModeButton.interactable ? Color.white : _disabledImageColor;
    }

    public bool IsGameModePlayable() {
        if (TowerTagSettings.Home) return true;
        switch (Mode) {
            case GameMode.GoalTower:
                return TowerTagSettings.Home || PlayerManager.Instance.GetParticipatingIcePlayerCount() >= 1
                    && PlayerManager.Instance.GetParticipatingFirePlayerCount() >= 1;
            default:
                return true;
        }
    }

    public void AddVoteIcon() {
        InstantiateWrapper.InstantiateWithMessage(_votePrefab, _voteLine.transform);
    }

    public void RemoveVoteIcon() {
        if (VoteCount > 0)
            Destroy(_voteLine.transform.GetChild(0).gameObject);
    }

    public void RemoveAllVoteIcons() {
        if (VoteCount <= 0) return;
        for (int i = 0; i < VoteCount; i++) {
            Destroy(_voteLine.transform.GetChild(i).gameObject);
        }
    }

    public void ToggleModeInfoOverlayText(bool status, string infoText = "") {
        if (!status) {
            _modeInfoOverlayText.gameObject.SetActive(false);
            return;
        }

        if (_modeInfoOverlayText == null) return;
        _modeInfoOverlayText.text = infoText;
        _modeInfoOverlayText.gameObject.SetActive(true);
        if (_modeInfoOverlayText.gameObject.activeSelf) _selectionLine.SetActive(false);
    }
}