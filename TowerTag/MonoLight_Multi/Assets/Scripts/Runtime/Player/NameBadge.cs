using System;
using System.Collections;
using JetBrains.Annotations;
using Photon.Voice.PUN;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class NameBadge : MonoBehaviour {
    [SerializeField] private TMP_Text _nameBadge;
    [SerializeField] private Image _voiceIcon;
    [SerializeField] private Color _friendlyColor;
    [SerializeField] private Color _enemyColor;
    [SerializeField] private Material _overlayMaterial;

    private IPlayer _player;
    private float _fadeDelay = 0.5f;
    private PhotonVoiceView _photonVoiceView;
    private bool _activeLastFrame;
    private Coroutine _detection;
    private Coroutine _fade;
    public TMP_Text Badge => _nameBadge;

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
        try {
            _nameBadge.gameObject.SetActive(false);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void OnMatchFinishedLoading(IMatch match) {
        try {
            _nameBadge.gameObject.SetActive(true);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void OnPlayerRevived(IPlayer player) {
        try {
            if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing) return;
            _nameBadge.gameObject.SetActive(true);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemy, byte colliderType) {
        try {
            _nameBadge.gameObject.SetActive(false);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void OnCommendationsSceneLoaded() {
        try {
            _nameBadge.gameObject.SetActive(false);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void OnConfigurationStarted() {
        try {
            _nameBadge.gameObject.SetActive(true);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public void Init(IPlayer player, [CanBeNull] PhotonVoiceView photonVoiceView) {
        _player = player;
        StartVoiceDetection(photonVoiceView);

        player.PlayerNameChanged += SetPlayerName;
        player.PlayerTeamChanged += UpdateColor;

        if (_player.PlayerHealth != null) {
            _player.PlayerHealth.PlayerDied += OnPlayerDied;
            _player.PlayerHealth.PlayerRevived += OnPlayerRevived;
        }

        if (TTSceneManager.Instance != null) {
            TTSceneManager.Instance.CommendationSceneLoaded += OnCommendationsSceneLoaded;
        }

        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        GameManager.Instance.MatchHasFinishedLoading += OnMatchFinishedLoading;
        GameManager.Instance.MatchConfigurationStarted += OnConfigurationStarted;

        IPlayer localPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (localPlayer != null)
            localPlayer.PlayerTeamChanged += OnLocalPlayerTeamChanged;

        if (SharedControllerType.IsAdmin)
            _nameBadge.fontSharedMaterial = _overlayMaterial;

        SetPlayerName(player.PlayerName);
        UpdateColor(player, player.TeamID);
    }

    private void OnEnable()
    {
        StartVoiceDetection(_photonVoiceView);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _fade = null;
        _detection = null;
    }

    private void OnDestroy() {
        if (_player != null) {
            _player.PlayerNameChanged -= SetPlayerName;
            _player.PlayerTeamChanged -= UpdateColor;
        }

        if (_player != null && _player.PlayerHealth != null) {
            _player.PlayerHealth.PlayerDied -= OnPlayerDied;
            _player.PlayerHealth.PlayerRevived -= OnPlayerRevived;
        }

        if (TTSceneManager.Instance != null) {
            TTSceneManager.Instance.CommendationSceneLoaded -= OnCommendationsSceneLoaded;
        }
        IPlayer localPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (localPlayer != null)
            localPlayer.PlayerTeamChanged -= OnLocalPlayerTeamChanged;

        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchFinishedLoading;
        GameManager.Instance.MatchConfigurationStarted -= OnConfigurationStarted;
    }

    private void OnLocalPlayerTeamChanged(IPlayer player, TeamID teamID) {
        try {
            if (_player != null)
                UpdateColor(_player, _player.TeamID);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void SetPlayerName(string playerName) {
        try {
            Badge.text = playerName;
            if (_player != null)
                UpdateColor(_player, _player.TeamID);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void UpdateColor(IPlayer player, TeamID teamID) {
        try {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (ownPlayer != null && _player != null) {
                Badge.color = teamID == ownPlayer.TeamID ? _friendlyColor : _enemyColor;
                _voiceIcon.color = teamID == ownPlayer.TeamID ? _friendlyColor : _enemyColor;
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public void ToggleVisibility(bool visible) {
        if(_fade != null)
            StopCoroutine(_fade);
        _fade = StartCoroutine(FadeText(visible));
    }

    private IEnumerator FadeText(bool visible) {
        if(!visible)
            yield return new WaitForSeconds(_fadeDelay);
        float time = 0;
        const float duration = 0.1f;
        Color startColor = _nameBadge.color;
        Color finalColor = new Color(startColor.r, startColor.g, startColor.b, visible ? 1 : 0);
        while (time <= duration) {
            _nameBadge.color = Color.Lerp(startColor, finalColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _nameBadge.color = finalColor;
        _fade = null;

    }

    private void StartVoiceDetection(PhotonVoiceView photonVoiceView)
    {
        if (photonVoiceView != null)
        {
            _photonVoiceView = photonVoiceView;
            if (_detection == null)
                _detection = StartCoroutine(VoiceDetection());
        }
    }

    private IEnumerator VoiceDetection()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (true)
        {
            if(_activeLastFrame != _photonVoiceView.IsSpeaking)
            {
                _voiceIcon.enabled = _photonVoiceView.IsSpeaking;
                _activeLastFrame = _photonVoiceView.IsSpeaking;
            }
            yield return wait;
        }
    }
}