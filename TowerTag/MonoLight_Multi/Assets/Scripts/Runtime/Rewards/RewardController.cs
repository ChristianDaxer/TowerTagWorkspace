using System.Collections;
using System.Collections.Generic;
using TowerTag;
using UnityEngine;
using ColliderTyp = DamageDetectorBase.ColliderType;

namespace Rewards {
    public class RewardController : MonoBehaviour {
        [SerializeField] private HitGameAction _hitAction;

        [SerializeField, Tooltip("The height of the animation")]
        private float _spawnDistanceToPlayer = 10f;

        [SerializeField, Tooltip("The GameObject for the animation")]
        private GameObject _rewardCanvas;

        [SerializeField, Tooltip("The order of the RewardTypes also is the priority (just one at the same time shown")]
        private Reward[] _reward;

        private Transform _localPlayerHead;

        //List of player who got hit and the localPlayers sees a Reward above them
        //-> There can be multiple Rewards at the same time but just one above one player
        private readonly List<IPlayer> _playerWithRunningReward = new List<IPlayer>();

        private Animator _anim;

        private Vector3 _hitPoint;

        private const int _maxRewards = 2;
        
        private List<GameObject> _rewards = new List<GameObject>(_maxRewards);

        private void OnEnable() {
            _hitAction.PlayerGotHit += DetectRewardType;
            RegisterListeners(GameManager.Instance);
        }

        private void OnDisable() {
            _hitAction.PlayerGotHit -= DetectRewardType;
            UnregisterListeners(GameManager.Instance);
        }

        private void RegisterListeners(GameManager gameManager) {
            gameManager.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
            if (GameManager.Instance.CurrentMatch != null)
                RemoveListener(GameManager.Instance.CurrentMatch);
        }

        private void UnregisterListeners(GameManager gameManager) {
            gameManager.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        }

        private void OnMatchHasFinishedLoading(IMatch match) {
            match.Finished += OnMatchFinished;
            match.RoundStartingAt += OnStartRoundAt;

            //Reset at the start of a match to be sure it is at 0
            OnStartRoundAt(match, -1);
        }


        private void RemoveListener(IMatch match) {
            match.Finished -= OnMatchFinished;
            match.RoundStartingAt -= OnStartRoundAt;
        }

        private void OnStartRoundAt(IMatch match, int time) {
            foreach (Reward rewardType in _reward) {
                rewardType.OnStartMatchAt(match, time);
            }
        }

        private void OnMatchFinished(IMatch match) {
            foreach (Reward rewardType in _reward) {
                rewardType.OnMatchFinished(match);
            }

            RemoveListener(match);
        }

        private void DetectRewardType(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, ColliderTyp colliderType) {
            if (TTSceneManager.Instance.IsInHubScene) return;

            //The reward animations are just for the local client and if the target was killed
            Reward rewardToDisplay = null;
            foreach (Reward rewardType in _reward) {
                bool rewardEarned = rewardType.HandleHit(shotData, targetPlayer, hitPoint, colliderType);
                if (rewardToDisplay == null && rewardEarned) {
                    rewardToDisplay = rewardType;
                }
            }

            //Reward animation part -> just on local client
            if (rewardToDisplay == null || !shotData.Player.IsMe) return;

            _localPlayerHead = shotData.Player.PlayerAvatar.Targets.Head;

            _hitPoint = hitPoint;
            Vector3 spawnPosition = CalculatePositionRelativeToPlayer();
            TriggerDetectedAnimation(spawnPosition, rewardToDisplay, targetPlayer);
        }

        private void TriggerDetectedAnimation(Vector3 spawnPosition, Reward reward, IPlayer targetPlayer) {
            //Check if the player who got hit already has a reward above
            if (!_playerWithRunningReward.Contains(targetPlayer)) {
               // GameObject rewardCanvas = Instantiate(_rewardCanvas, spawnPosition, Quaternion.identity);
                GameObject rewardCanvas = GetRewardFromPool(spawnPosition);

                _anim = rewardCanvas.GetComponentInChildren<Animator>();

                reward.PlaySound(rewardCanvas.transform);

                _playerWithRunningReward.Add(targetPlayer);
                StartCoroutine(PlayDetectedAnimation(reward.AnimatorStateName, rewardCanvas, targetPlayer));
            }
        }
        #region Pool rewards
        private GameObject GetRewardFromPool(Vector3 pos)
        {
            
            GameObject rewardCanvas = null;
            int max = _rewards.Count;
            int capacity = _rewards.Capacity;
           // Debug.Log("GetRewardFromPool max " + max + " capacity " + capacity);

            if (max < capacity)
            {
                rewardCanvas = InstantiateWrapper.InstantiateWithMessage(_rewardCanvas, pos, Quaternion.identity);
                _rewards.Add(rewardCanvas);
            }
            else if(max == capacity)
            { 
                for(int i= 0; i < max; i++ )
                {
                    rewardCanvas = _rewards[i];
                    if (!rewardCanvas.activeSelf)
                        break;
                }
            }

            if (rewardCanvas)
            {
                rewardCanvas.SetActive(true);
                rewardCanvas.transform.position = pos;
                rewardCanvas.transform.rotation = Quaternion.identity;
            }
 

            return rewardCanvas;
        }

        private void ReturnToPool(GameObject reward)
        {
            reward.SetActive(false);
        }
        #endregion

        private IEnumerator PlayDetectedAnimation(string animatorStateName, GameObject rewardCanvas, IPlayer targetPlayer) {
            _anim.Play(animatorStateName);
            while (AnimatorIsPlaying()) {
                yield return null;
            }

            //We don't want to have the Canvas anymore after animation is played
            // Destroy(rewardCanvas);
            ReturnToPool(rewardCanvas);
            _playerWithRunningReward.Remove(targetPlayer);
        }

        public Vector3 CalculatePositionRelativeToPlayer() {
            Vector3 localPlayerHeadPosition = _localPlayerHead.position;
            Vector3 normalizedVectorBetween = Vector3.Normalize(_hitPoint - localPlayerHeadPosition);
            return localPlayerHeadPosition + _spawnDistanceToPlayer * normalizedVectorBetween;
        }

        private bool AnimatorIsPlaying() {
            return _anim.GetCurrentAnimatorStateInfo(0).length >
                   _anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
    }
}