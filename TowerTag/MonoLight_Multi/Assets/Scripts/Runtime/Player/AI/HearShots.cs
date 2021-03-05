using UnityEngine;

namespace AI {
    /// <summary>
    /// Simple Helper Class. Script has to be attached to the Bot's hearing radius.
    /// Adds heard shots to the BotBrain.
    /// </summary>
    public class HearShots : MonoBehaviour {
        [SerializeField] private BotBrain _botBrain;

        public void Hear(Shot shot) {

            if (_botBrain == null) {
                Debug.LogErrorFormat("Missing reference to {0} on {1} component attached to GameObject: \"{2}\".", nameof(BotBrain), nameof(HearShots), gameObject.name);
                return;
            }

            _botBrain.HearShot(shot);
        }
    }
}
