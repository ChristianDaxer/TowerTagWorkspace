using TowerTag;
using UnityEngine;
using ColliderType = DamageDetectorBase.ColliderType;

namespace Rewards {
    /// <summary>
    /// A player gets a realtime reward when he did something special (e.g. Headshot)
    /// 
    /// <author>Patrick Wienzek (patrick@vrnerds.de )</author>
    /// </summary>
    public abstract class Reward : ScriptableObject {
        [SerializeField, Tooltip("The name of the AnimatorState in the Animator of the RewardCanvas that has to be triggered when the reward is earned")]
        private string _animatorStateName;

        [SerializeField] private AudioClip _sound;

        public string AnimatorStateName => _animatorStateName;

        public abstract bool HandleHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, ColliderType colliderType);

        // TODO: remove return; is crossing with "team x lost 1 player"
        public void PlaySound(Transform target) {
            if (_sound == null) {
                Debug.LogWarning("There is no sound assigned to the reward!");
                return;
            }

            AudioSource source = target.GetComponent<AudioSource>();
            source.clip = _sound;
            source.Play();
        }

        public virtual void OnStartMatchAt(IMatch match, int time) {
        }

        public virtual void OnMatchFinished(IMatch match) {
        }

        public virtual void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        }
    }
}