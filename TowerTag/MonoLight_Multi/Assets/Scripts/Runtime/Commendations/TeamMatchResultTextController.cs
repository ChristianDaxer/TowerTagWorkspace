using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.XR;

namespace Commendations {
    /// <summary>
    /// Controls the Text component that displays the team match result.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class TeamMatchResultTextController : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private string _team0WinsText = "TEAM FIRE WINS THE MATCH";
        [SerializeField] private string _team1WinsText = "TEAM ICE WINS THE MATCH";
        [SerializeField] private string _tieText = "THE MATCH IS A TIE";

        private void Start() {
            if (GameManager.Instance.CurrentMatch == null) {
                Debug.LogError("There is no current match! Disabling TeamMatchResultTextController");
                return;
            }

            MatchStats currentMatchStats = GameManager.Instance.CurrentMatch.Stats;
            if (currentMatchStats == null) {
                Debug.LogError("Cannot display correct match result: no GameModeStats");
                return;
            }

            UpdateText(currentMatchStats);
            if (XRSettings.enabled) {
                Transform thisTransform = transform;
                Vector3 localScale = thisTransform.localScale;
                localScale =
                    new Vector3(-localScale.x, localScale.y, localScale.z);
                thisTransform.localScale = localScale;
            }
        }

        private void UpdateText(MatchStats currentMatchStats) {
            switch (currentMatchStats.WinningTeamID) {
                case TeamID.Fire:
                    _text.text = _team0WinsText;
                    _text.color = TeamManager.Singleton.TeamFire.Colors.UI;
                    break;
                case TeamID.Ice:
                    _text.text = _team1WinsText;
                    _text.color = TeamManager.Singleton.TeamIce.Colors.UI;
                    break;
                default:
                    _text.text = _tieText;
                    _text.color = TeamManager.Singleton.TeamNeutral.Colors.UI;
                    break;
            }
        }
    }
}