using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class AbortMatchVoteToggleButton : MonoBehaviour
{
    [SerializeField] private Toggle _abortMatchToggle;
    private Text _abortMatchText;

    public IPlayer OwnPlayer { get; private set; }

    private void Awake()
    {
        OwnPlayer = PlayerManager.Instance.GetOwnPlayer();
        _abortMatchText = _abortMatchToggle.GetComponentInChildren<Text>();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        _abortMatchToggle.onValueChanged.AddListener(AbortMatchToggled);
        _abortMatchToggle.isOn = OwnPlayer.AbortMatchVote;
    }

    private void OnDisable()
    {
        _abortMatchToggle.onValueChanged.RemoveListener(AbortMatchToggled);
    }
    
    private void Update()
    {
        if (GameManager.Instance.CurrentMatch != null && !GameManager.Instance.CurrentMatch.MatchStarted)
            return;
        if (gameObject.activeInHierarchy)
            _abortMatchText.text = AbortMatchVotingController.GetAbortMatchText();
    }
    
    private void AbortMatchToggled(bool value)
    {
        OwnPlayer.AbortMatchVote = value;

        if (value)
        {
            OwnPlayer.PlayerNetworkEventHandler.SendAbortMatchVoted();
        }
    }
}
