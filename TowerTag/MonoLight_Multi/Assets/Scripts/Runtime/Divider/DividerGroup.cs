using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TowerTag.Divider {
    public class DividerGroup : MonoBehaviour {
        private int _simultaneouslyActiveHighlights = 1;
        [SerializeField] private float _roundRunningBlinksPerSecond = 0.125f;
        [SerializeField] private float _roundFinishedBlinksPerSecond = 2f;
        private List<DividerHighlight> _highlights;

        private void OnEnable() {
            _highlights = GetComponentsInChildren<DividerHighlight>().ToList();
            if (GameManager.Instance.CurrentMatch == null) return;
            GameManager.Instance.CurrentMatch.StartingAt += StartNewRoundHighlight;
            GameManager.Instance.CurrentMatch.Finished += StartMatchFinishedHighlight;
            GameManager.Instance.CurrentMatch.RoundStartingAt += StartNewRoundHighlight;
            GameManager.Instance.CurrentMatch.RoundFinished += StartRoundFinishedShow;
        }

        private void OnDisable() {
            StopAllCoroutines();
            if (GameManager.Instance.CurrentMatch == null)
                return;
            GameManager.Instance.CurrentMatch.StartingAt -= StartNewRoundHighlight;
            GameManager.Instance.CurrentMatch.Finished -= StartMatchFinishedHighlight;
            GameManager.Instance.CurrentMatch.RoundStartingAt -= StartNewRoundHighlight;
            GameManager.Instance.CurrentMatch.RoundFinished -= StartRoundFinishedShow;
        }

        private void StartMatchFinishedHighlight(IMatch match) {
            StopAllCoroutines();
            ResetDividerHighlights();
            if (_roundFinishedBlinksPerSecond > 0) {
                StartCoroutine(Pulse(_simultaneouslyActiveHighlights, 1 / _roundFinishedBlinksPerSecond,
                    _roundFinishedBlinksPerSecond, match.Stats.WinningTeamID == TeamID.Neutral
                        ? (TeamID?) null
                        : match.Stats.WinningTeamID));
            }
        }

        public void StartNewRoundHighlight(IMatch match, int time) {
            StopAllCoroutines();
            ResetDividerHighlights();
            if (_roundRunningBlinksPerSecond > 0) {
                StartCoroutine(Pulse(_simultaneouslyActiveHighlights, 1 / _roundRunningBlinksPerSecond,
                    _roundRunningBlinksPerSecond));
            }
        }

        public void StartRoundFinishedShow(IMatch match, TeamID roundWinningTeamID) {
            StopAllCoroutines();
            ResetDividerHighlights();
            if (_roundFinishedBlinksPerSecond > 0) {
                StartCoroutine(Pulse(_simultaneouslyActiveHighlights, 1 / _roundFinishedBlinksPerSecond,
                    _roundFinishedBlinksPerSecond, roundWinningTeamID));
            }
        }

        public void StopRoundFinishedShow() {
            StopAllCoroutines();
            ResetDividerHighlights();
        }

        private IEnumerator Pulse(int dividerCount, float duration, float blinksPerSecond, TeamID? teamID = null) {
            ResetDividerHighlights();
            if (_highlights.Count < dividerCount) yield break;
            if (teamID != null) _highlights.ForEach(div => div.ColorizeInWinningTeamColor((TeamID) teamID));

            var wait = new WaitForSeconds(duration / 2);
            float[] time = new float[_highlights.Count];

            while (true) {

                float nextTime = 0.0f;
                for (int i = 0; i < _highlights.Count; i++)
                {
                    int randomI = Random.Range(0, _highlights.Count - 1);
                    nextTime = 0.0f;

                    if (!_highlights[randomI].CurrentlyHighlighting)
                    {
                        time[randomI] = 0.0f;
                        _highlights[randomI].StartPulse(duration, blinksPerSecond, teamID);
                    }

                    time[randomI] += Time.deltaTime;
                    nextTime += Time.deltaTime;
                    _highlights[randomI].TickPulse(time[randomI]);

                    float randomWaitTime = Random.Range(0, 2);

                    WaitForSeconds waitForSeconds = new WaitForSeconds(randomWaitTime);
                    yield return waitForSeconds;
                }

                /*
                _highlights
                    .Where(highlight => !highlight.CurrentlyHighlighting)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(dividerCount)
                    .ForEach(highlight => highlight.StartPulse(duration, blinksPerSecond, true, teamID));
                */

                yield return wait;
            }
        }

        private void ResetDividerHighlights() {
            _highlights.ForEach(highlight => highlight.Reset());
        }
    }
}