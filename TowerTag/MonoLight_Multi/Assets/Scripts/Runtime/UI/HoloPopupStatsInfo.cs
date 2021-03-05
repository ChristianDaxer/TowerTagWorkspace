using System;
using Home;
//using Steamworks;
using TowerTag;
using UnityEngine;
using TowerTagAPIClient;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine.UI;
using VRNerdsUtilities;

public struct StatsInfoData
{
    public float Score;
    public int Rank;
    public float XP;
    public int Level;

    public int HealthHealed;
    public int PillarsClaimed;
    public int Assists;
    public int Kills;

    public float ScorePrevious;
    public int RankPrevious;
    public float XPPrevious;
    public int LevelPrevious;
    
    public int HealthHealedPrevious;
    public int PillarsClaimedPrevious;
    public int AssistsPrevious;
    public int KillsPrevious;

    public float RankDifference => Rank > RankPrevious ? Rank - RankPrevious : -(RankPrevious - Rank);
    public int KillsDifference => Kills > KillsPrevious ? Kills - KillsPrevious : -(KillsPrevious - Kills);
    public int AssistsDifference => Assists > AssistsPrevious ? Assists - AssistsPrevious : -(AssistsPrevious - Assists);
    public int HealingDifference => HealthHealed > HealthHealedPrevious ? HealthHealed - HealthHealedPrevious : -(HealthHealedPrevious - HealthHealed);
    public int ClaimsDifference => PillarsClaimed > PillarsClaimedPrevious ? PillarsClaimed - PillarsClaimedPrevious : -(PillarsClaimedPrevious - PillarsClaimed);

    public float ScoreDifference => Score > ScorePrevious ? Score - ScorePrevious : -(ScorePrevious - Score);

    public float XPDifference => XP > XPPrevious ? XP - XPPrevious : -(XPPrevious - XP);

    public float LevelDifference => Level > LevelPrevious ? Level - LevelPrevious : -(LevelPrevious - Level);
}

public class HoloPopupStatsInfo : SingletonMonoBehaviour<HoloPopupStatsInfo>
{
    public delegate void StatisticsInfoDelegate(StatsInfoData data);

    public static event Action OnStatisticsRequested;
    public static event StatisticsInfoDelegate OnStatisticsReceived;

    [Header("Display Texts")] [SerializeField]
    private Text _score;

    [SerializeField] private Text _scoreAddition;
    [SerializeField] private Text _rank;
    [SerializeField] private Text _rankAddition;
    [SerializeField] private Text _xp;
    [SerializeField] private Text _xpAddition;
    [SerializeField] private Text _level;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioSource _finishedSource;

    [Header("Display Bars")] [SerializeField]
    private Image _redIndicator;

    [SerializeField] private Image _blueIndicator;

    private int _currentLevel;
    private int _previousLevel;
    private StatsInfoData _currentStatsInfo;
    private PlayerStatistics _previousPlayerStatistics;
    private LeaderboardEntry _previousLeaderboardEntry;
    private static readonly int _color = Shader.PropertyToID("_Color");
    private static readonly int FresnelColor = Shader.PropertyToID("_FresnelColor");
    private Color _startAdditionColor;

    private void Start()
    {
        _currentStatsInfo = new StatsInfoData();
        var material = _scoreAddition.material;
        _startAdditionColor = material.GetColor(FresnelColor);
        CreateTmpMaterials();
    }

    private void OnDisable()
    {
        PlayerStatisticsStore.PlayerStatisticsReceived -= OnPlayerStatisticsReceived;
        /* TODO
        if(TowerTagSettings.IsHomeTypeSteam)
            SteamLeaderboardManager.OnPlayerDataDownloaded -= OnPlayerDataDownloaded;
        else if(TowerTagSettings.IsHomeTypeViveport)
            ViveportLeaderboardManager.LocalEntryReceived -= OnPlayerDataDownloaded;
        */
    }

    private void CreateTmpMaterials()
    {
        InitTmpMaterialFromText(_rankAddition);
        InitTmpMaterialFromText(_scoreAddition);
        InitTmpMaterialFromText(_xpAddition);
    }

    private void InitTmpMaterialFromText(Text text)
    {
        Material tmpMat = new Material(text.material);
        text.material = tmpMat;
    }

    public void ResetColors()
    {
        Color color = _startAdditionColor;
        color.a = 0;
        _scoreAddition.material.SetColor(_color, color);
        _scoreAddition.material.SetColor(FresnelColor, _startAdditionColor);
        _rankAddition.material.SetColor(FresnelColor, _startAdditionColor);
        _rankAddition.material.SetColor(_color, color);
        _xpAddition.material.SetColor(FresnelColor, _startAdditionColor);
        _xpAddition.material.SetColor(_color, color);
        _redIndicator.color = TeamManager.Singleton.TeamFire.Colors.UI;
    }

    #region Statistics Request

    public void LoadData(PlayerStatistics previousStatistics, LeaderboardEntry previousLeaderboardEntry)
    {
        _previousPlayerStatistics = previousStatistics;
        _previousLeaderboardEntry = previousLeaderboardEntry;

        PlayerStatisticsStore.ClearStatisticsCache();

        RequestStatistics();
        PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
        /* TODO
        if(TowerTagSettings.IsHomeTypeSteam)
            SteamLeaderboardManager.OnPlayerDataDownloaded += OnPlayerDataDownloaded;
        if (TowerTagSettings.IsHomeTypeViveport)
            ViveportLeaderboardManager.LocalEntryReceived += OnPlayerDataDownloaded;
        */
    }

    private void OnPlayerDataDownloaded(Viveport.Leaderboard obj) {
        _currentStatsInfo.Rank = obj.Rank;
        OnStatisticsReceived?.Invoke(_currentStatsInfo);
    }

    private void OnPlayerDataDownloaded(LeaderboardEntry entry)
    {
        _currentStatsInfo.Rank = entry.Rank;
        OnStatisticsReceived?.Invoke(_currentStatsInfo);
        //Debug.LogError($"PREVIOUS: Level: {_currentStatsInfo.LevelPrevious}, Score: {_currentStatsInfo.ScorePrevious}, XP: {_currentStatsInfo.XPPrevious}, Rank: {_currentStatsInfo.RankPrevious}");
        //Debug.LogError($"RECEIVED: Level: {_currentStatsInfo.Level}, Score: {_currentStatsInfo.Score}, XP: {_currentStatsInfo.XP}, Rank: {_currentStatsInfo.Rank}");
    }

    private void OnPlayerStatisticsReceived(PlayerStatistics stats)
    {
        if (stats == null) return;
        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            return;
        if (!stats.id.Equals(playerIdManager.GetUserId())) return;
        _currentStatsInfo.Score = stats.rating;
        _currentStatsInfo.Level = stats.level;
        _currentStatsInfo.XP = stats.xp;
        _currentStatsInfo.Assists = stats.totalAssists;
        _currentStatsInfo.Kills = stats.totalScore;
        _currentStatsInfo.PillarsClaimed = stats.totalPillarsClaimed;
        _currentStatsInfo.HealthHealed = stats.totalHealthHealed;

        _currentStatsInfo.RankPrevious = _previousLeaderboardEntry.Rank;
        if (_previousPlayerStatistics != null) {
            _currentStatsInfo.AssistsPrevious = _previousPlayerStatistics.totalAssists;
            _currentStatsInfo.KillsPrevious = _previousPlayerStatistics.totalScore;
            _currentStatsInfo.HealthHealedPrevious = _previousPlayerStatistics.totalHealthHealed;
            _currentStatsInfo.PillarsClaimedPrevious = _previousPlayerStatistics.totalPillarsClaimed;
            _currentStatsInfo.ScorePrevious = _previousPlayerStatistics.rating;
            _currentStatsInfo.LevelPrevious = _previousPlayerStatistics.level;
            _currentStatsInfo.XPPrevious = _previousPlayerStatistics.xp;
        }

        /* TODO
        if(TowerTagSettings.IsHomeTypeSteam)
            SteamLeaderboardManager.Instance.GetLeaderboardUsers();
        else if(TowerTagSettings.IsHomeTypeViveport)
            ViveportLeaderboardManager.Instance.GetLocalPlayerEntry();
        */
    }

    private void RequestStatistics()
    {
        OnStatisticsRequested?.Invoke();
        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            return;
        PlayerStatisticsStore.GetStatistics(Authentication.OperatorApiKey, playerIdManager.GetUserId());
    }

    #endregion

    #region Interface Update

    public void ChangeInterfaceStatValues(StatsInfoData data)
    {
        _blueIndicator.fillAmount = PlayerAccount.CalculateProgressOfLevel(data.XPPrevious, out _previousLevel);
        SetInterfaceStatValue(_score, data.ScorePrevious, _scoreAddition, data.ScoreDifference);
        SetInterfaceStatValue(_rank, data.RankPrevious, _rankAddition, data.RankDifference, true);
        SetInterfaceStatValue(_xp, data.XPPrevious, _xpAddition, data.XPDifference);
        SetInterfaceStatValue(_level, data.LevelPrevious);
    }

    public void StartAdditions(StatsInfoData data)
    {
        // Closing popup after 15s
        LeanTween.delayedCall(25f, () => { transform.parent.GetComponent<HoloPopUp>().ResetHoloPopUpAnimation(); });

        StartAddition(_score, data.ScorePrevious, _scoreAddition, data.ScoreDifference);

        LeanTween.delayedCall(3.5f,
            () => { StartAddition(_rank, data.RankPrevious, _rankAddition, data.RankDifference, true); });

        LeanTween.delayedCall(6f, () =>
        {
            StartAddition(_xp, data.XPPrevious, _xpAddition, data.XPDifference);
            SetInterfaceProgressBarStatValue(_redIndicator, data.XP);
            StartAddition(_level, data.Level, null, data.LevelDifference);
        });
    }

    private void SetInterfaceStatValue(Text text, float value, Text additionText = null, float addition = 0f,
        bool negate = false)
    {
        //if (addition >= 0)
        text.text = Mathf.RoundToInt(value).ToString();

        if (additionText)
        {
            additionText.text = GetStringFieldNumberWithPrefix(addition, negate);
        }
    }

    private void SetInterfaceProgressBarStatValue(Image progressBar, float xp)
    {
        float xpProgressNormalizedOld = _blueIndicator.fillAmount;
        float xpProgressNormalized = PlayerAccount.CalculateProgressOfLevel(xp, out _currentLevel);

        int progressedLevels = _currentLevel - _previousLevel;

        (progressedLevels != 0
                ? LevelsSkippedProgressAnimation(progressBar, progressedLevels)
                : NoLevelsSkippedProgressAnimation(progressBar, xpProgressNormalizedOld))
            .setOnComplete(() =>
            {
                LeanTween.value(gameObject, progressedLevels == 0 ? xpProgressNormalizedOld : 0f, xpProgressNormalized,
                    progressedLevels == 0 ? 3.2f : 1f).setOnUpdate(
                    v => { progressBar.fillAmount = v; }).setOnComplete(() =>
                {
                    LeanTween.scale(_redIndicator.gameObject, _redIndicator.transform.localScale * 1.05f, 0.5f)
                        .setEasePunch();
                    LeanTween.scale(_blueIndicator.gameObject, _blueIndicator.transform.localScale * 1.05f, 0.5f)
                        .setEasePunch();
                    LeanTween.delayedCall(0.1f, () =>
                    {
                        _redIndicator.color = TeamManager.Singleton.TeamNeutral.Colors.UI;
                        _finishedSource.Play();
                    });
                });
            });
    }

    private LTDescr LevelsSkippedProgressAnimation(Image progressBar, int progressedLevels)
    {
        int iteration = 0;

        return LeanTween.value(gameObject, 0f, 1f, 1f).setLoopCount(progressedLevels)
            .setOnUpdate(v =>
            {
                progressBar.fillAmount = v;

                if (Math.Abs(v - 1f) < 0.05f)
                {
                    LeanTween.scale(_redIndicator.gameObject, _redIndicator.transform.localScale * 1.05f, 0.5f)
                        .setEasePunch();
                    iteration++;
                }

                if (iteration == 1) _blueIndicator.fillAmount = 0f;
            });
    }

    private LTDescr NoLevelsSkippedProgressAnimation(Image progressBar, float normalizedXpOld)
    {
        return LeanTween.value(gameObject, 0f, 0f, 0f);
    }

    private void StartAddition(Text valueText, float value, Text additionText, float additionValue, bool negate = false)
    {
        Color initialColor = _scoreAddition.material.GetColor(_color);

        if (additionText)
        {
            additionText.gameObject.SetActive(true);
        }

        LeanTween.delayedCall(1f, () =>
        {
            if (additionText)
            {
                additionText.text = GetStringFieldNumberWithPrefix(Mathf.RoundToInt(additionValue), negate);
            }
        });

        LeanTween.delayedCall(Mathf.RoundToInt(additionValue) == 0 ? 0.5f : 0f, () =>
        {
            LeanTween.value(0f, 1f, 0.5f).setOnUpdate(v =>
            {
                initialColor.a = v;
                if (additionText)
                {
                    additionText.material.SetColor(_color, initialColor);
                }
            }).setOnComplete(() =>
            {
                LeanTween.delayedCall(0.5f, () =>
                {
                    string previousValueText = Mathf.RoundToInt(value).ToString();

                    LeanTween.value(gameObject, 0f, Math.Abs(additionValue),
                            Mathf.RoundToInt(additionValue) == 0 ? 0.5f : 2f)
                        .setOnUpdate(v =>
                        {
                            valueText.text = Mathf.RoundToInt(additionValue >= 0f ? value + v : value - v).ToString();

                            if (previousValueText != valueText.text)
                            {
                                _source.Play();
                            }

                            previousValueText = valueText.text;
                        })
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(valueText.gameObject, valueText.transform.localScale * 1.1f, 0.8f)
                                .setEase(LeanTweenType.punch);

                            if (additionText)
                            {
                                additionText.material.SetColor(FresnelColor, TeamManager.Singleton.TeamIce.Colors.Effect);
                            }
                        });
                });
            });
        });
    }

    private string GetStringFieldNumberWithPrefix(float addition, bool negate = false)
    {
        if (negate) addition = -addition;

        var prefix = addition >= 0f ? "+" : "-";
        var gain = Mathf.RoundToInt(Mathf.Abs(addition));

        return $"{prefix}{gain}";
    }

    #endregion
}