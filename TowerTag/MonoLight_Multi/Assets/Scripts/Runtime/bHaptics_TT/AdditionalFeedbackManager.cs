using Bhaptics.Tact.Unity;
using System.Collections;
using TowerTag;
using UnityEngine;

public class AdditionalFeedbackManager : MonoBehaviour {
    [SerializeField, Tooltip("Will be played the playerHealth is below _lowHealthFeedValue")]
    private TactSource _lowHealth;

    [SerializeField, Tooltip("If CurrentHealth is after a hit below this value, the feedback will start")]
    private int _lowHealthFeedbackValue = 40;

    [SerializeField, Tooltip("Wait time between the heart beats")]
    private float _heartBeatPause = 2;

    [SerializeField, Tooltip("Will be played when the player dies")]
    private TactSource _afterDeath;

    [SerializeField] private TactSource _headShotVest;

    [SerializeField, Tooltip("Will be played when the match time is in the countdown state on every new second")]
    private TactSource _countDown;

    [SerializeField, Tooltip("Will be played when the countdown finished")]
    private TactSource _countDownFinished;

    [SerializeField, Tooltip("Will be played the Player is out of Chaperone or in tower")]
    private TactSource _outOfBounds;

    [SerializeField, Tooltip("Will be played the Player is out of Chaperone or in tower")]
    private TactSource _outOfBoundsVest;

    [SerializeField, Tooltip("Will be played the Player is out of Chaperone or in tower")]
    private TactSource _sleeveShot;

    [SerializeField, Tooltip("Will be played the Player is out of Chaperone or in tower")]
    private TactSource _ropeConnect;

    [SerializeField, Tooltip("Will be played the Player is out of Chaperone or in tower")]
    private TactSource _charge;

    [SerializeField, Tooltip("Delay between the vibrations")]
    private float _outOfBoundsDelay = 1;

    [SerializeField] private float _claimInterval = 1;

    [SerializeField] private HitGameAction _hitGameAction;
    [SerializeField] private HoloPopUp _holoPopUp;

    private DisabledGunControllerVisuals _disabledGunControllerVisuals;
    private Coroutine _heartBeatCoroutine;
    private Coroutine _outOfBoundsCoroutine;
    private Coroutine _afterDeathCoroutine;
    private IPlayer _player;
    private Coroutine _chargeCoroutine;

    private const float DelayForFollowFeedback = 0.5f;
    private const int MaxOutOfBoundsCount = 5;

    [SerializeField] private RopeGameAction ropeGameAction;

    private void OnEnable() {
        if (_player == null)
            _player = GetComponentInParent<IPlayer>();

        RegisterEventListener();
    }

    private void OnDisable() {
        UnregisterEventListener();
    }

    private void RegisterEventListener() {
        //Vest Events
        RegisterVestEvents();
        RegisterSleeveEvents();
    }

    private void RegisterSleeveEvents() {
        _player.GunController.ShotTriggered += OnShotTriggered;
        ropeGameAction.RopeConnectedToChargeable += OnRopeConnected;
        ropeGameAction.Disconnecting += OnRopeDisconnecting;
    }

    private void RegisterVestEvents() {
        _player.OutOfChaperoneStateChanged += TriggerOutOfBounds;
        _player.InTowerStateChanged += TriggerOutOfBounds;
        _hitGameAction.PlayerGotHit += TriggerAfterHitFeedback;
        GameManager.Instance.MatchHasFinishedLoading += AddDeactivateHeartBeatOnRoundFinished;
        // if (_holoPopUp != null) {
        //     _holoPopUp.DisplayedCountdownTimeChanged += PlayCountdownFeedback;
        //     _holoPopUp.CountdownFinished += PlayGoFeedback;
        // }
    }

    private void UnregisterEventListener() {
        UnregisterVestListener();
        UnregisterSleeveEvents();
    }

    private void UnregisterSleeveEvents() {
        //Sleeve events
        _player.GunController.ShotTriggered -= OnShotTriggered;
        ropeGameAction.RopeConnectedToChargeable -= OnRopeConnected;
        ropeGameAction.Disconnecting -= OnRopeDisconnecting;
    }

    private void UnregisterVestListener() {
        //Vest Events
        _hitGameAction.PlayerGotHit -= TriggerAfterHitFeedback;
        _player.OutOfChaperoneStateChanged -= TriggerOutOfBounds;
        _player.InTowerStateChanged -= TriggerOutOfBounds;
        GameManager.Instance.MatchHasFinishedLoading -= AddDeactivateHeartBeatOnRoundFinished;
        // if (_holoPopUp != null) {
        //     _holoPopUp.CountdownFinished -= PlayGoFeedback;
        //     _holoPopUp.DisplayedCountdownTimeChanged -= PlayCountdownFeedback;
        // }
    }

    private void OnRopeDisconnecting(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose) {
        if (player.IsMe)
            StopCoroutine(_chargeCoroutine);
    }

    private void OnRopeConnected(RopeGameAction sender, IPlayer player, Chargeable pillar) {
        if (player.IsMe) {
            _ropeConnect.Play();
            _chargeCoroutine = StartCoroutine(ChargeVibration());
        }
    }

    private IEnumerator ChargeVibration() {
        while (true) {
            if (!_charge.IsPlaying())
                _charge.Play();
            yield return new WaitForSeconds(_claimInterval);
        }
    }

    private void OnShotTriggered() {
        _sleeveShot.Play();
    }

    //Triggers Feedback when ouf of chaperone or in tower!
    private void TriggerOutOfBounds(object sender, bool outOfBounce) {
        if (outOfBounce)
            _outOfBoundsCoroutine = StartCoroutine(OutOfBounceFeedback());
        else {
            if (_outOfBoundsCoroutine != null)
                StopCoroutine(_outOfBoundsCoroutine);
        }
    }

    //If the last living Player has the Heartbeat activated then we have to stop it
    private void AddDeactivateHeartBeatOnRoundFinished(IMatch match) {
        match.RoundFinished += DeactivateHeartBeat;
    }

    private void DeactivateHeartBeat(IMatch obj, TeamID teamID) {
        if (_heartBeatCoroutine != null) {
            StopCoroutine(_heartBeatCoroutine);
            _heartBeatCoroutine = null;
        }
    }


    private void PlayGoFeedback() {
        _countDownFinished.Play();
    }

    private void PlayCountdownFeedback() {
        _countDown.Play();
    }


    private void TriggerAfterHitFeedback(
        ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        if (targetPlayer.IsMe) {
            //Tried to solve this with the TactSources on the ShotPrefab, but instead of playing als sources in the
            //sender it plays a random of the list (e.g. multiple head sources -> only one will be played)
            if (targetType == DamageDetectorBase.ColliderType.Head)
                _headShotVest.Play();

            int playerHealth = targetPlayer.PlayerHealth.CurrentHealth;
            //Once i tried to use PlayerHealth.IsAlive and it did not work because in some cases the health gets set to
            //MaxHealth after death. This is why this condition is used.
            if (playerHealth >= 100 || playerHealth <= 0) {
                if (_afterDeathCoroutine == null)
                    _afterDeathCoroutine = StartCoroutine(AfterDeathFeedback(shotData.TactSender));
            }
            // else if (playerHealth <= _lowHealthFeedbackValue && targetPlayer.PlayerHealth.IsActive) {
            //     if (_heartBeatCoroutine != null)
            //         StopCoroutine(_heartBeatCoroutine);
            //
            //     _heartBeatCoroutine = StartCoroutine(LowHealthFeedback(shotData.TactSender));
            // }
        }
    }

    private IEnumerator AfterDeathFeedback(TactSender tactSender) {
        if (tactSender != null) {
            if (_afterDeath == null) {
                Debug.LogError("No afterDeath TactSource found!");
                yield break;
            }

            yield return new WaitForSeconds(DelayForFollowFeedback);

            if (_heartBeatCoroutine != null) {
                StopCoroutine(_heartBeatCoroutine);
                _heartBeatCoroutine = null;
            }

            _afterDeath.Play();
            yield return new WaitForSeconds(DelayForFollowFeedback);
            _afterDeathCoroutine = null;
        }
    }

    private IEnumerator LowHealthFeedback(TactSender tactSender) {
        if (tactSender != null) {
            if (_lowHealth == null) {
                Debug.LogError("No lowHealth TactSource found!");
                yield break;
            }

            while (true) {
                yield return new WaitForSeconds(_heartBeatPause);
                _lowHealth.Play();
            }
        }
    }


    private IEnumerator OutOfBounceFeedback() {
        if (_outOfBounds == null) {
            Debug.LogError("No outOfBounce TactSource found!");
            yield break;
        }
        int outOfBoundsCount = 0;
        while (outOfBoundsCount <= MaxOutOfBoundsCount) {
            yield return new WaitForSeconds(_outOfBoundsDelay);
            _outOfBounds.Play();
            _outOfBoundsVest.Play();
            outOfBoundsCount++;
        }
    }
}