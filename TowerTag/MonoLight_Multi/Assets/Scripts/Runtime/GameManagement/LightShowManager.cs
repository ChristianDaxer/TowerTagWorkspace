using TowerTag;
using UnityEngine;
using VLB;

namespace Home
{
    public class LightShowManager : MonoBehaviour
    {
        [SerializeField] private Animator[] _laserShowAnimators;
        [SerializeField] private VolumetricLightBeam[] _laserBeams;
        private static readonly int _round = Animator.StringToHash("Round");
        private static readonly int _match = Animator.StringToHash("Match");

        private void OnEnable()
        {
            if(GameManager.Instance.CurrentMatch != null)
            {
                GameManager.Instance.CurrentMatch.RoundFinished += OnRoundFinished;
                GameManager.Instance.CurrentMatch.Finished += OnMatchFinished;
            }
            else
            {
                Debug.LogError("No Match found. Can't register on match events");
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance.CurrentMatch == null) return;
            GameManager.Instance.CurrentMatch.RoundFinished -= OnRoundFinished;
            GameManager.Instance.CurrentMatch.Finished -= OnMatchFinished;
        }

        private void OnRoundFinished(IMatch match, TeamID roundWinningTeamId)
        {
            _laserShowAnimators.ForEach(spot => spot.SetTrigger(_round));
            ColorizeLightsOfSpot(TeamManager.Singleton.Get(roundWinningTeamId).Colors.Main);
        }

        private void OnMatchFinished(IMatch match)
        {
            _laserShowAnimators.ForEach(spot => spot.SetTrigger(_match));
            ColorizeLightsOfSpot(TeamManager.Singleton.Get(match.Stats.WinningTeamID).Colors.Main);
        }

        private void ColorizeLightsOfSpot(Color color)
        {
            _laserBeams.ForEach(beam => beam.color = color);
        }


        /// <summary>
        /// Use this in the editor to fill the array in the prefab!
        /// </summary>
        [ContextMenu("GetLightBeams")]
        public void GetLightBeams()
        {
            _laserBeams = GetComponentsInChildren<VolumetricLightBeam>(true);
        }
    }
}