using System.Collections;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class StayLoggedInTrigger : MonoBehaviour {
    [SerializeField] private OptionSelector[] _options;
    [SerializeField] private Text _infoText;

    public Text InfoText => _infoText;

    private const int TimeForLogOut = 20;
    private float _timeLeft;
    private bool _optionCharged;
    public bool OptionCharging { get; set; }

    public OptionSelector[] Options => _options;

    private void Start() {
        for (var i = 0; i < Options.Length; i++) {
            Options[i].ID = i;
        }
    }

    private void OnEnable() {
        Options.ForEach(option => option.gameObject.SetActive(true));
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        _timeLeft = TimeForLogOut;
        InfoText.text = "STAY LOGGED IN?\n";
    }

    private void OnDisable() {
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
        _options[0].Trigger();
    }

    private void Update() {
        if (SharedControllerType.IsAdmin) return;

        if (!OptionCharging && _timeLeft >= 0 && !_optionCharged) {
            _timeLeft -= Time.deltaTime;
            InfoText.text = $"STAY LOGGED IN?\n {Mathf.CeilToInt(_timeLeft)}s";
        } else if(_timeLeft < 0){
            _options[1].Trigger();
        }
    }

    public IEnumerator DisplayConfirmationText(string text) {
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        _optionCharged = true;
        InfoText.text = text;
        Options.ForEach(option => option.gameObject.SetActive(false));
        yield return new WaitForSeconds(2);

        _optionCharged = false;
        gameObject.SetActive(false);
    }
}
