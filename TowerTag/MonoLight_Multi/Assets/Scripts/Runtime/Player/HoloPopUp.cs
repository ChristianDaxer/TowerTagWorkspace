using System;
using System.Collections;
using Home;
using JetBrains.Annotations;
using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using TowerTagAPIClient;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using VRNerdsUtilities;
using Match = TowerTag.Match;
using MatchStore = TowerTagAPIClient.Store.MatchStore;

public class HoloPopUp : SingletonMonoBehaviour<HoloPopUp> {
    public event Action DisplayedCountdownTimeChanged;
    public event Action CountdownFinished;

    [Header("Actions")] [SerializeField] private SteamVR_Action_Boolean _menuButtonAction;

    [Header("Movement Settings")] [SerializeField]
    private Transform _rayCastLookAtTarget;

    [SerializeField] private Transform _positionAnchor;
    [SerializeField] private Camera _hmdCamera;
    [SerializeField] private float _holoPopUpSmoothTime = 0.3f;

    [Header("UI")] [SerializeField] private Text _playerName;
    [SerializeField] private Text _teamName;
    [SerializeField] private Text _countdownText;
    [SerializeField] private Text _roundText;

    [Header("Audio & Animation")] [SerializeField]
    private AudioClip _spawnSound;

    [SerializeField] private AudioClip _despawnSound;
    [SerializeField] private Animator _animator;
    [SerializeField] private ParticleSystem _rays;
    [SerializeField] private GameObject _holoLightRays;
    

    private bool IsCurrentMatchRunning => GameManager.Instance.CurrentMatch != null
                                          && GameManager.Instance.CurrentMatch.MatchStarted;

    private LeaderboardEntry _previousPlayerLeaderboardEntry;
    private PlayerStatistics _previousLocalStats;
    private StatsInfoData _currentData;

    private ParticleSystem _emission;
    private Vector3 _holoPopUpVelocity;

    private Coroutine _currentCountdownTimer;

    private AudioSource _source;
    private IPlayer _player;
    private bool _animatorIsRunning;

    public bool AnimatorIsRunning {
        set => _animatorIsRunning = value;
    }

    private string _currentAnimatorClipName;
    private float _currentAnimatorClipTime;

    private string CurrentAnimatorClipName {
        get {
            if (_animator != null) {
                var clips = _animator.GetCurrentAnimatorClipInfo(0);
                if (clips == null || clips.Length < 1) return "";
                var clip = clips[0];
                _currentAnimatorClipTime = clip.clip.length;
                _currentAnimatorClipName = clip.clip.name;
            }
            else
                _currentAnimatorClipName = "";

            return _currentAnimatorClipName;
        }
    }

    private static readonly int SpawnMatchInfo = Animator.StringToHash("spawnMatchInfo");
    private static readonly int DespawnMatchInfo = Animator.StringToHash("despawnMatchInfo");
    private static readonly int SpawnLateJoinInfo = Animator.StringToHash("spawnLateJoinInfo");
    private static readonly int DespawnLateJoinInfo = Animator.StringToHash("despawnLateJoinInfo");
    private static readonly int SpawnMenuInfo = Animator.StringToHash("spawnMenuInfo");
    private static readonly int DespawnMenuInfo = Animator.StringToHash("despawnMenuInfo");
    private static readonly int SpawnStatsInfo = Animator.StringToHash("spawnStatsInfo");
    private static readonly int DespawnStatsInfo = Animator.StringToHash("despawnStatsInfo");
    private static readonly int SpawnLoading = Animator.StringToHash("spawnLoading");
    private static readonly int DespawnLoading = Animator.StringToHash("despawnLoading");
    private static readonly int Reset = Animator.StringToHash("reset");

    public delegate void PlayerReceivedMatchHandler(string id);

    public static PlayerReceivedMatchHandler OnPlayerMatchReceived;

    private IPlayer Player {
        get => _player;
        set {
            _player = value;
            if (_player != null) {
                UnregisterEventListeners();
                RegisterEventListeners();
            }
        }
    }

    private enum InfoType {
        MatchInfo,
        LateJoinerInfo,
        MenuInfo,
        StatsInfo,
        Loading
    }

    private new void Awake() {
        Player = GetComponentInParent<IPlayer>();
        _source = GetComponent<AudioSource>();
        _emission = _rays;
        ResetHoloPopUpAnimation();
    }


    private void OnEnable() {
        UnregisterEventListeners();
        RegisterEventListeners();
        if (PlayerAccount.ReceivedPlayerStatistics) _previousLocalStats = PlayerAccount.Statistics;
    }

    private void OnDisable() {
        ResetHoloPopUpAnimation();
        UnregisterEventListeners();
    }

    private void Update() {
        if (IsAnimationInDefaultState()) return;
        if(!_rays.IsNull())
            _rays.gameObject.transform.LookAt(_rayCastLookAtTarget);

        if (PlayerHeadBase.GetInstance(out var playerHeadBase))
            gameObject.transform.LookAt(playerHeadBase.HeadCamera.transform);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            _positionAnchor.position,
            ref _holoPopUpVelocity,
            _holoPopUpSmoothTime);
    }

    private void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (!TTSceneManager.Instance.IsInConnectScene && !TTSceneManager.Instance.IsInTutorialScene) return;
            TriggerAnimation(!IsActive, InfoType.MenuInfo);
        }
    }

    private void RegisterEventListeners() {
        if (_menuButtonAction != null) {
            if (SharedControllerType.VR) {
                _menuButtonAction.AddOnStateDownListener(MenuButtonClicked, SteamVR_Input_Sources.RightHand);
                _menuButtonAction.AddOnStateDownListener(MenuButtonClicked, SteamVR_Input_Sources.LeftHand);
            }
        }

        if (Player != null) {
            Player.CountdownStarted += ActivateCountdown;
            Player.ParticipatingStatusChanged += OnParticipatingStatusChanged;
            //OnParticipatingStatusChanged(Player, Player.IsParticipating);
        }

        TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
        TTSceneManager.Instance.CommendationSceneLoaded += OnCommendationSceneLoaded;
        PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        // SteamLeaderboardManager.OnPlayerDataDownloaded += OnLeaderboardPlayerDataDownloaded; TODO

        MatchStore.MatchReported += OnMatchPosted;
        OnPlayerMatchReceived += OnMatchPostedClient;
    }

    private void MenuButtonClicked(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        if (!TTSceneManager.Instance.IsInConnectScene && !TTSceneManager.Instance.IsInTutorialScene) return;
        TriggerAnimation(!IsActive, InfoType.MenuInfo);
    }

    private void UnregisterEventListeners() {
        if (_menuButtonAction != null) {
            if (SharedControllerType.VR) {
                _menuButtonAction.RemoveOnStateDownListener(MenuButtonClicked, SteamVR_Input_Sources.RightHand);
                _menuButtonAction.RemoveOnStateDownListener(MenuButtonClicked, SteamVR_Input_Sources.LeftHand);
            }
        }

        if (Player != null) {
            Player.CountdownStarted -= ActivateCountdown;
            Player.ParticipatingStatusChanged -= OnParticipatingStatusChanged;
            if (TTSceneManager.Instance != null)
                TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
        }

        MatchStore.MatchReported -= OnMatchPosted;
        PlayerStatisticsStore.PlayerStatisticsReceived -= OnPlayerStatisticsReceived;
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        if (TTSceneManager.Instance)
            TTSceneManager.Instance.CommendationSceneLoaded -= OnCommendationSceneLoaded;


        MatchStore.MatchReported -= OnMatchPosted;
        OnPlayerMatchReceived -= OnMatchPostedClient;
    }

    private void OnHubSceneLoaded() {
        ResetHoloPopUpAnimation();
    }

    private void OnParticipatingStatusChanged(IPlayer player, bool isParticipating) {
        if (IsCurrentMatchRunning) {
            TriggerAnimation(!isParticipating, InfoType.LateJoinerInfo);
            TriggerAnimation(isParticipating, InfoType.MatchInfo);

            if (isParticipating) {
                GameManager.Instance.CurrentMatch.Finished += OnMatchFinished;
            }
            else {
                GameManager.Instance.CurrentMatch.Finished -= OnMatchFinished;
            }
        }
    }


    private void OnMatchFinished(IMatch match) {
        if (!Player.IsParticipating) {
            TriggerAnimation(false, InfoType.LateJoinerInfo);
            match.Finished -= OnMatchFinished;
        }
    }

    /// <summary>
    /// When the duration is > 0 it will start a Countdown after a delay in duration's length
    /// </summary>
    private void ActivateCountdown(int startAtTime, int countdownTypeInt) {
        if (!Player.IsParticipating) return;
        var countdownType = (Match.CountdownType) countdownTypeInt;
        var countdownDuration = GetCountdownDurationByType(countdownType);

        SetRoundText();


        // deactivate player -> activate limbo (saturation = minSaturation)
        int timestampToStartCountdown = startAtTime - (countdownDuration * 1000);

        SetUpTextFields(countdownDuration);
        transform.position = _positionAnchor.position;
        if (IsAnimationInDefaultState())
            TriggerAnimation(true, InfoType.MatchInfo);
        if (_currentCountdownTimer != null) {
            StopCoroutine(_currentCountdownTimer);
        }


        // show reactivation countdown timer to player

        _currentCountdownTimer =
            StartCoroutine(ShowReactivationCountdown(countdownDuration, timestampToStartCountdown));
    }

    private void SetRoundText() {
        if (TTSceneManager.Instance.IsInTutorialScene || TTSceneManager.Instance.IsInHubScene
                                                      || GameManager.Instance.CurrentMatch != null &&
                                                      GameManager.Instance.CurrentMatch.IsActive)
            _roundText.text = "RESPAWN IN";
        else {
            _roundText.text = "ROUND STARTS IN";
        }
    }

    private static int GetCountdownDurationByType(Match.CountdownType countdownType) {
        if (countdownType == Match.CountdownType.StartMatch
            || countdownType == Match.CountdownType.ResumeMatch)
            return GameManager.Instance.MatchStartCountdownTimeInSec;
        if (countdownType == Match.CountdownType.StartRound)
            return GameManager.Instance.RoundStartCountdownTimeInSec;
        return 0;
    }

    private void TriggerAnimation(bool active, InfoType infoType) {
        if (_animatorIsRunning) return;


        switch (infoType) {
            case InfoType.MatchInfo:
                _animator.SetTrigger(active ? SpawnMatchInfo : DespawnMatchInfo);
                break;
            case InfoType.LateJoinerInfo:
                _animator.SetTrigger(active ? SpawnLateJoinInfo : DespawnLateJoinInfo);
                break;
            case InfoType.MenuInfo:
                _animator.SetTrigger(active ? SpawnMenuInfo : DespawnMenuInfo);
                break;
            case InfoType.StatsInfo:
                _animator.SetTrigger(active ? SpawnStatsInfo : DespawnStatsInfo);
                break;
            case InfoType.Loading:
                _animator.SetTrigger(active ? SpawnLoading : DespawnLoading);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
        }

        _source.clip = active ? _spawnSound : _despawnSound;
        IsActive = active;
        UpdateHoloLightRaysState();
        _source.Play();
        ParticleSystem.EmissionModule emissionModule;
        emissionModule = _emission.emission;
        emissionModule.enabled = true;
    }

    public void UpdateHoloLightRaysState()
    {
        if (_holoLightRays == null) return;
        if(IsActive) _holoLightRays.SetActive(true);
        else Invoke(nameof(DeActiveHoloLightRays), _currentAnimatorClipTime);
    }
    
    public void DeActiveHoloLightRays() => _holoLightRays.SetActive(false);

    public void ResetHoloPopUpAnimation()
    {
        if (_animator == null) return;

        // Spawn Match Info Holo Popup
        _animator.ResetTrigger(SpawnMatchInfo);
        _animator.ResetTrigger(DespawnMatchInfo);

        // Spawn late join info Holo Popup
        _animator.ResetTrigger(SpawnLateJoinInfo);
        _animator.ResetTrigger(DespawnLateJoinInfo);

        // Spawn Menu Info Holo Popup
        _animator.ResetTrigger(SpawnMenuInfo);
        _animator.ResetTrigger(DespawnMenuInfo);

        // Spawn Stats Info Holo Popup
        _animator.ResetTrigger(SpawnStatsInfo);
        _animator.ResetTrigger(DespawnStatsInfo);

        var clip = CurrentAnimatorClipName;

        if (clip.Equals("HoloPopUp_In"))
            _animator.SetTrigger(DespawnMatchInfo);
        else if (clip.Equals("MenuInfo"))
            _animator.SetTrigger(DespawnMenuInfo);
        else if (clip.Equals("LateJoinInfo"))
            _animator.SetTrigger(DespawnLateJoinInfo);
        else if (clip.Equals("StatsInfo_In"))
            _animator.SetTrigger(DespawnStatsInfo);
        else if (clip.Equals("Loading"))
            _animator.SetTrigger(Reset);
        else if (clip.Equals(""))
            _animator.SetTrigger(Reset);

        IsActive = false;
        AnimatorIsRunning = false;
        ParticleSystem.EmissionModule emissionModule;
        emissionModule = _emission.emission;
        emissionModule.enabled = false;
    }

    #region Testing

    private void TestResetHoloPopUpAnimation(string clip)
    {
        if (_animator == null) return;

        // Spawn Match Info Holo Popup
        _animator.ResetTrigger(SpawnMatchInfo);
        _animator.ResetTrigger(DespawnMatchInfo);

        // Spawn late join info Holo Popup
        _animator.ResetTrigger(SpawnLateJoinInfo);
        _animator.ResetTrigger(DespawnLateJoinInfo);

        // Spawn Menu Info Holo Popup
        _animator.ResetTrigger(SpawnMenuInfo);
        _animator.ResetTrigger(DespawnMenuInfo);


        if (clip.Equals("HoloPopUp_In"))
            _animator.SetTrigger(DespawnMatchInfo);
        else if (clip.Equals("MenuInfo"))
            _animator.SetTrigger(DespawnMenuInfo);
        else if (clip.Equals("LateJoinInfo"))
            _animator.SetTrigger(DespawnLateJoinInfo);
        else if (clip.Equals(""))
            _animator.SetTrigger(Reset);

        IsActive = false;
        AnimatorIsRunning = false;
        ParticleSystem.EmissionModule emissionModule;
        emissionModule = _emission.emission;
        emissionModule.enabled = false;
    }

    [ContextMenu("TestReset")]
    public void TestReset() {
        TestResetHoloPopUpAnimation("");
    }

    [ContextMenu("TestResetMatchInfo")]
    public void TestResetSpawnMatchInfo() {
        TestResetHoloPopUpAnimation("HoloPopUp_In");
    }

    [ContextMenu("TestResetMenuInfo")]
    public void TestResetMenuInfo() {
        TestResetHoloPopUpAnimation("MenuInfo");
    }

    [ContextMenu("TestResetLateJoinInfo")]
    public void TestResetLateJoinInfo() {
        TestResetHoloPopUpAnimation("LateJoinInfo");
    }

    #endregion

    private bool IsActive { get; set; }

    private void SetUpTextFields(int countdownDuration) {
        _playerName.text = Player.PlayerName.ToUpper();
        _teamName.text = TeamManager.Singleton.Get(Player.TeamID).Name.ToUpper();
        _teamName.material = TeamMaterialManager.Singleton.GetHoloUI(Player.TeamID);
        _countdownText.text = "0" + countdownDuration + " SEC";
    }

    /// <summary>
    /// Show reactivation countdown timer to player (on VR Controller).
    /// </summary>
    /// <param name="duration">Countdown length.</param>
    /// <param name="timestampToStartCountdown">Time to start countdown</param>
    /// <returns></returns>
    private IEnumerator ShowReactivationCountdown(int duration, int timestampToStartCountdown) {
        if (_countdownText == null)
            yield break;

        if (duration <= 0)
            yield break;

        while (GameManager.Instance.IsInLoadMatchState)
            yield return null;

        while (timestampToStartCountdown >= PhotonNetwork.ServerTimestamp) {
            yield return null;
        }

        float startTime = Time.realtimeSinceStartup;
        int countDownTimeInSeconds = duration;
        int currentlyShownSecondsLeft = 0;

        while (countDownTimeInSeconds >= 0) {
            countDownTimeInSeconds = Mathf.CeilToInt(duration - (Time.realtimeSinceStartup - startTime));
            if (HasCountdownFinished(countDownTimeInSeconds)) {
                _countdownText.text = "GO!";
                TriggerAnimation(false, InfoType.MatchInfo);
                CountdownFinished?.Invoke();
                _currentCountdownTimer = null;
                yield break;
            }

            if (currentlyShownSecondsLeft != Mathf.Max(countDownTimeInSeconds, 0)) {
                currentlyShownSecondsLeft = Mathf.Max(countDownTimeInSeconds, 0);
                _countdownText.text = "0" + currentlyShownSecondsLeft + " SEC";
                DisplayedCountdownTimeChanged?.Invoke();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private bool HasCountdownFinished(int remainingSeconds) {
        return Mathf.Max(remainingSeconds, 0) == 0;
    }

    private bool IsAnimationInDefaultState() {
        return CurrentAnimatorClipName == "defaultstate";
    }

    #region AnimationEventListener

    [UsedImplicitly]
    public void OnHoloPopUpStarted() {
        if (CurrentAnimatorClipName == "MenuInfo")
            _animatorIsRunning = true;
        if (CurrentAnimatorClipName == "LateJoinInfo")
            _animatorIsRunning = true;
        if (CurrentAnimatorClipName == "HoloPopUp_In")
            _animatorIsRunning = true;
    }

    [UsedImplicitly]
    public void OnHoloPopUpFinished() {
        if (CurrentAnimatorClipName == "MenuInfo")
            _animatorIsRunning = false;
        if (CurrentAnimatorClipName == "LateJoinInfo")
            _animatorIsRunning = false;
        if (CurrentAnimatorClipName == "HoloPopUp_In")
            _animatorIsRunning = false;
    }

    public void OnMatchPostedClient(string id) {
        if (PhotonNetwork.IsMasterClient) return;
        print(id + " received!");
        HoloPopupStatsInfo.OnStatisticsReceived += OnStatisticsReceived;
        LeanTween.delayedCall(2f,
            () => { HoloPopupStatsInfo.Instance.LoadData(_previousLocalStats, _previousPlayerLeaderboardEntry); });
    }

    #endregion

    #region Stats Info

    public void OnMatchPosted(string id) {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.LogError(id + " sent!");

        _player.PlayerNetworkEventHandler.BroadcastPlayedMatch(id);
        HoloPopupStatsInfo.OnStatisticsReceived += OnStatisticsReceived;
        LeanTween.delayedCall(2f,
            () => { HoloPopupStatsInfo.Instance.LoadData(_previousLocalStats, _previousPlayerLeaderboardEntry); });
    }

    private void OnStatisticsReceived(StatsInfoData data) {
        _currentData = data;

        TriggerAnimation(false, InfoType.Loading);

        TriggerAnimation(true, InfoType.StatsInfo);

        HoloPopupStatsInfo.Instance.ChangeInterfaceStatValues(data);
        HoloPopupStatsInfo.OnStatisticsReceived -= OnStatisticsReceived;
        /*Debug.Log(
            $"score:{data.Score} oldscore:{data.ScorePrevious} rank:{data.Rank} oldrank:{data.RankPrevious} xp:{data.XP} oldxp:{data.XPPrevious} lvl:{data.Level} oldlvl:{data.LevelPrevious}");
        */
    }

    [UsedImplicitly]
    public void StartStatsInfoAdditions() {
        HoloPopupStatsInfo.Instance.StartAdditions(_currentData);
    }

    private void OnMissionBriefingStarted(MatchDescription _, GameMode gameMode) {
        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            return;
        PlayerStatisticsStore.GetStatistics(Authentication.OperatorApiKey, playerIdManager.GetUserId());
        /* TODO
        if (TowerTagSettings.IsHomeTypeSteam)
            SteamLeaderboardManager.Instance.GetLeaderboardUsers();
        */
    }

    private void OnPlayerStatisticsReceived(PlayerStatistics stats) {
        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            return;
        if (stats.id.Equals(playerIdManager.GetUserId())) {
            _previousLocalStats = stats;
        }
    }

    private void OnLeaderboardPlayerDataDownloaded(LeaderboardEntry entry) {
        _previousPlayerLeaderboardEntry = entry;
    }


    private void OnCommendationSceneLoaded() {
        // TODO: Find a reliable solution for determining player was NOT playing in the match and late joined
        var player = PlayerManager.Instance.GetOwnPlayer();

        bool didPlayerParticipate = false;
            if(player != null && GameManager.Instance.CurrentMatch != null)
                didPlayerParticipate = GameManager.Instance.CurrentMatch.Stats.GetPlayerStats().ContainsKey(player.PlayerID);
        if (didPlayerParticipate && !GameManager.Instance.TrainingVsAI) {
            TriggerAnimation(true, InfoType.Loading);
            HoloPopupStatsInfo.Instance.ResetColors();
        }
        else {
            Debug.Log("You were not participating in the match! Stats Popup wont be shown!");
        }
    }

    #endregion
}