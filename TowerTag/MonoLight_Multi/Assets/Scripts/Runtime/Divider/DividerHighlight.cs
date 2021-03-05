using System.Collections;
using UnityEngine;

namespace TowerTag.Divider {
    [RequireComponent(typeof(IDivider))]
    public class DividerHighlight : MonoBehaviour {
        private IDivider _divider;
        private IDivider Divider => _divider ?? (_divider = GetComponent<IDivider>());

        [SerializeField] private AnimationCurve _highlightCurve =
            new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        public bool CurrentlyHighlighting { get; private set; }

        public void Reset() {
            StopAllCoroutines();
            CurrentlyHighlighting = false;
            Divider.ResetHighlight();
        }

        /*
        public void StartPulse(float duration, float blinksPerSecond, bool randomPhase = true, TeamID? winningTeamId = null) {
            StartCoroutine(Pulse(duration, blinksPerSecond, randomPhase, winningTeamId));
        }
        */

        private float blinksPerSecond;
        private float duration;
        private TeamID? teamID;

        public void StartPulse(float duration, float blinksPerSecond, TeamID? teamID, bool randomPhase = true)
        {
            CurrentlyHighlighting = true;
            this.blinksPerSecond = blinksPerSecond;
            this.duration = duration;
            this.teamID = teamID;
#if UNITY_EDITOR
            StartCoroutine(Pulse());
#endif
        }

        public void TickPulse(float time)
        {
            float value = _highlightCurve.Evaluate(time * blinksPerSecond % 1f);
            if (teamID != null) Divider.SetHighlight(value, (TeamID)teamID);
            else Divider.SetHighlight(value);
            if (time > duration)
                FinishPulse(teamID);
        }

        public void FinishPulse(TeamID? teamID)
        {
            float finalValue = _highlightCurve.Evaluate(0);
            if (teamID != null)
                Divider.SetHighlight(finalValue, (TeamID)teamID);
            else Divider.SetHighlight(finalValue);
            CurrentlyHighlighting = false;
        }

#if UNITY_EDITOR
        private IEnumerator Pulse()
        {
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                float value = _highlightCurve.Evaluate(time * blinksPerSecond % 1f);
                if (teamID != null) Divider.SetHighlight(value, (TeamID)teamID);
                else Divider.SetHighlight(value);
                yield return null;
            }

            FinishPulse(teamID);
        }
#endif
        /*
        private IEnumerator Pulse(float duration, float blinksPerSecond, bool randomPhase, TeamID? teamID) {
            CurrentlyHighlighting = true;
            // random phase cut due to lacking performance
            //if(randomPhase) yield return new WaitForSeconds(Random.Range(0, 1 / blinksPerSecond));
            float time = 0;
            float deltaTime = Time.deltaTime;
            while (time <= duration) {
                time += deltaTime;
                float value = _highlightCurve.Evaluate(time * blinksPerSecond % 1f);
                if (teamID != null) Divider.SetHighlight(value, (TeamID) teamID);
                else Divider.SetHighlight(value);
                yield return null;
            }
            float finalValue = _highlightCurve.Evaluate(0);
            if (teamID != null)
                Divider.SetHighlight(finalValue, (TeamID)teamID);
            else
                Divider.SetHighlight(finalValue);
            CurrentlyHighlighting = false;
        }
        */

        public void ColorizeInWinningTeamColor(TeamID winningTeamID) {
            Divider.SetHighlight(0, winningTeamID);
        }
    }
}