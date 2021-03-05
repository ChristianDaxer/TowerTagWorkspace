using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Toornament.DataTransferObject;
using Toornament.Store;
using Toornament.Store.Model;
using UnityEngine;
using VRNerdsUtilities;

public class ToornamentImgui : SingletonMonoBehaviour<ToornamentImgui> {
    [SerializeField] private int _padding;
    [SerializeField] private int _totalHeight;
    [SerializeField] private int _mainAreaHeight;
    [SerializeField] private int _mainAreaWidth;
    [SerializeField] private int _innerAreaHeight;
    [SerializeField] private int _textAreaHeight;
    [SerializeField] private int _textAreaWidth;
    [SerializeField] private Vector2 _position;
    [SerializeField] private Vector2 _textPosition;
    [SerializeField] private KeyCode _triggerOverlayKeyCode;

    // cached UI data
    private object[] _currentData = { };
    private readonly Dictionary<object, int> _indices = new Dictionary<object, int>();
    private readonly Dictionary<object, Vector2> _scrollPositions = new Dictionary<object, Vector2>();
    private bool _visible;
    private Type _currentType;

    // cached Toornament data
    private string _tournamentId;
    private string _matchId;
    private string _infoText;
    private string _date = DateTime.Today.ToString("yyyy-MM-dd");

    // events
    public event Action<Match> OnSelectMatch;

    private void Start() {
        Debug.Log("Init auth store for convenience: " + AuthStore.Instance);
        transform.parent = ToornamentContainer.Instance.transform;
        TournamentStore.Instance.OnGetTournaments += ShowDataSet;
        TournamentStore.Instance.OnGetTournament += ShowSingleDataItem;
        TournamentStore.Instance.OnCreateTournament += ShowSingleDataItem;
        TournamentStore.Instance.OnUpdateTournament += ShowAsText;
        TournamentStore.Instance.OnDeleteTournament += DeleteTournament;
        TournamentStore.Instance.OnError += ShowAsText;
        MatchStore.Instance.OnGetMatches += ShowDataSet;
        MatchStore.Instance.OnGetMatch += ShowSingleDataItem;
        MatchStore.Instance.OnPatchMatch += ShowAsText;
        MatchStore.Instance.OnGetMatchResult += (matchId, result) => ShowAsText(result);
        MatchStore.Instance.OnPutMatchResult += (matchId, result) => ShowAsText(result);
        MatchStore.Instance.OnError += ShowAsText;
        ParticipantStore.Instance.OnGetParticipants += ShowDataSet;
        ParticipantStore.Instance.OnUpdateParticipant += ShowAsText;
        ParticipantStore.Instance.OnRegisterParticipant += ShowSingleDataItem;
        ParticipantStore.Instance.OnError += ShowAsText;
    }

    private void Update() {
        if (Input.GetKeyDown(_triggerOverlayKeyCode)) _visible = !_visible;
    }

    private void DeleteTournament(string tournamentId) {
        _currentData = new object[] { };
    }

    private void ShowSingleDataItem(object obj) {
        if (obj.GetType() == _currentType) {
            _currentData = new[] {obj};
        }
    }

    private void ShowDataSet<T>(T[] data) {
        if (typeof(T[]) == _currentType) {
            _currentData = data.Select(match => match as object).ToArray();
        }
    }

    public void ShowAsText(string text) {
        _infoText = _infoText + text + "\n" + "\n";
    }

    private void ShowAsText(object obj) {
        _infoText = _infoText + JsonConvert.SerializeObject(obj) + "\n" + "\n";
    }

    private void OnGUI() {
        if (!_visible) return;
        GUILayout.BeginArea(new Rect(_position.x + _padding, _position.y + _padding, _mainAreaWidth, _totalHeight));
        if (GUILayout.Button("Public Tournaments")) {
            _currentType = typeof(Tournament[]);
            TournamentStore.Instance.GetTournaments();
        }

        if (GUILayout.Button("My Tournaments")) {
            _currentType = typeof(Tournament[]);
            TournamentStore.Instance.GetMyTournaments();
        }

        GUILayout.BeginHorizontal();
        _date = GUILayout.TextField(_date);
        if (GUILayout.Button("Current TowerTag Tournaments")) {
            _currentType = typeof(Tournament[]);
            TournamentStore.Instance.GetTournaments("tower_tag", _date);
        }

        GUILayout.EndHorizontal();

//        if (GUILayout.Button("Create New Tournament")) {
//            _currentData = new object[] {CreateTournamentDTO.Dummy()};
//        }

        if (_currentData.Length > 0) {
            RenderObject(_currentData, _mainAreaHeight);
        }

        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(
            _position.x + _mainAreaWidth + _padding,
            _position.y + _padding,
            _textAreaWidth,
            _textAreaHeight));
        _textPosition = GUILayout.BeginScrollView(_textPosition);
        GUILayout.TextArea(_infoText);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void RenderObject(object obj, int height) {
        if (obj.GetType().IsArray) {
            RenderArray(obj as object[], height);
        }
        else {
            RenderSingleObject(obj, height);
        }
    }

    private void RenderArray(object[] list, int height) {
        GUILayout.BeginVertical();
        if (list.Length == 0) return;
        if (!_indices.ContainsKey(list)) {
            _indices[list] = 0;
        }

        RenderObject(list[_indices[list]], height);
        if (list.Length > 1) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous")) {
                _indices[list] = (_indices[list] + list.Length - 1) % list.Length;
            }

            if (GUILayout.Button("Next")) {
                _indices[list] = (_indices[list] + 1) % list.Length;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void RenderSingleObject(object obj, int height) {
        GUILayout.BeginVertical("box");
        if (!_scrollPositions.ContainsKey(obj)) {
            _scrollPositions[obj] = Vector2.zero;
        }

        _scrollPositions[obj] = GUILayout.BeginScrollView(_scrollPositions[obj], GUILayout.Height(height));
        foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties()) {
            if (!propertyInfo.IsDefined(typeof(ShowInUI), true)) continue;
            object value = propertyInfo.GetValue(obj, null);
            GUILayout.BeginHorizontal();
            GUILayout.Label(propertyInfo.Name);
            Type propType = value == null ? propertyInfo.PropertyType : value.GetType();
            propType = propType.UnderlyingSystemType;
            if (propType == typeof(string)) {
                propertyInfo.SetValue(obj, GUILayout.TextField(value as string), null);
            }

            else if (propType == typeof(bool?) || propType == typeof(bool)) {
                value = value ?? false;
                propertyInfo.SetValue(obj, GUILayout.Toggle((bool) value, ""), null);
            }

            else if (propType == typeof(int) || propType == typeof(int?)) {
                string stringValue = GUILayout.TextField("" + value);
                if (stringValue == "") propertyInfo.SetValue(obj, null, null);
                else propertyInfo.SetValue(obj, int.Parse(stringValue), null);
            }

            else if (propType == typeof(string[])) {
                var list = value as string[];
                string s = list == null ? "" : list.Aggregate((a, b) => a + "," + b);
                propertyInfo.SetValue(obj, GUILayout.TextField(s).Split(',').ToArray(), null);
            }

            else if (propType == typeof(int[])) {
                var list = value as int[];
                string s = list == null
                    ? ""
                    : list
                        .Select(integer => integer.ToString())
                        .Aggregate((a, b) => a + "," + b);
                propertyInfo.SetValue(obj, GUILayout.TextField(s).Split(',')
                    .Select(int.Parse)
                    .ToArray(), null);
            }

            else {
                if (value != null) RenderObject(value, _innerAreaHeight);
            }

            GUILayout.EndHorizontal();
        }

        switch (obj) {
            // data type specific buttons
            case Tournament _:
                RenderTournamentButtons();
                break;
            case Match match:
                RenderMatchButtons(match);
                break;
            case CreateTournamentDTO dto:
                RenderCreateTournamentButtons(dto);
                break;
            case RegisterParticipantDTO participantDTO:
                RenderCreateParticipantButtons(participantDTO);
                break;
            case Participant participant:
                RenderParticipantButtons(participant);
                break;
            case MatchResult result:
                RenderMatchResultButtons(result);
                break;
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void RenderTournamentButtons() {
        var tournament = _currentData[_indices[_currentData]] as Tournament;
        if (tournament == null) return;
        _tournamentId = tournament.id;

        if (!string.IsNullOrEmpty(_tournamentId) && GUILayout.Button("Update")) {
            TournamentStore.Instance.PatchTournament(tournament.id, tournament);
        }

        if (!string.IsNullOrEmpty(_tournamentId) && GUILayout.Button("Participants")) {
            _currentType = typeof(Participant[]);
            ParticipantStore.Instance.GetParticipants(_tournamentId);
        }

        if (tournament.id != "" && GUILayout.Button("Matches")) {
            _currentType = typeof(Match[]);
            // matches come without participant lineup, so participant details will be needed
            ParticipantStore.Instance.GetParticipants(tournament.id);
            MatchStore.Instance.GetMatches(tournament.id);
        }

//        if (tournament.id != "" && GUILayout.Button("Delete")) {
//            TournamentStore.Instance.DeleteTournament(tournament.id);
//        }
    }

    private void RenderMatchButtons(Match match) {
        if (match == null) return;
        _matchId = match.id;
        if (GUILayout.Button("Report Match Results")) {
            _currentType = null;
            _currentData = new object[] {
                new MatchResult {
                    opponents = match.opponents,
                    status = match.status
                }
            };
        }

        if (GUILayout.Button("Select")) {
            OnSelectMatch?.Invoke(match);
        }

        if (GUILayout.Button("Back")) {
            _currentType = typeof(Tournament);
            TournamentStore.Instance.GetTournament(_tournamentId);
        }
    }

    private void RenderMatchResultButtons(MatchResult matchResult) {
        if (matchResult == null) return;
        if (GUILayout.Button("Send")) {
            MatchStore.Instance.PutMatchResult(_tournamentId, _matchId, matchResult);
        }

        if (GUILayout.Button("Back")) {
            _currentType = typeof(Match[]);
            MatchStore.Instance.GetMatches(_tournamentId);
        }
    }

    private void RenderCreateParticipantButtons(RegisterParticipantDTO registerParticipant) {
        if (GUILayout.Button("Add Team Participant")) {
            _currentType = typeof(Participant);
            ParticipantStore.Instance.RegisterParticipant(registerParticipant);
        }
    }

    private void RenderCreateTournamentButtons(CreateTournamentDTO createTournamentDTO) {
        if (GUILayout.Button("Create Tournament")) {
            _currentType = typeof(Tournament);
            TournamentStore.Instance.CreateTournament(createTournamentDTO);
        }
    }

    private void RenderParticipantButtons(Participant participant) {
        if (GUILayout.Button("Update Participant")) {
            if (participant == null) return;
            ParticipantStore.Instance.PatchParticipant(_tournamentId, participant.id, new CreateParticipantDTO {
                email = participant.email,
                lineup = participant.lineup == null
                    ? new CreateParticipantDTO.TeamMember[] { }
                    : participant.lineup.Select(
                        teamMember => new CreateParticipantDTO.TeamMember {
                            email = teamMember.email,
                            name = teamMember.name
                        }).ToArray(),
                name = participant.name
            });
        }
    }
}