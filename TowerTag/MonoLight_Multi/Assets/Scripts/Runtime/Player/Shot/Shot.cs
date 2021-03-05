using System;
using Bhaptics.Tact.Unity;
using UnityEngine;
using AI;
using TowerTag;

/// <summary>
/// A shot from a Tower Tag laser gun. This script manages the movement of the projectile and hit detection.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
public class Shot : MonoBehaviour {
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _flyByThresholdDistance = 10;
    [SerializeField] private ShotManager _shotManager;
    [SerializeField] private HitGameAction _hitGameAction;
    [SerializeField] private float _lifeTime;
    [SerializeField] private FlyByFake _flyBy;
    [SerializeField] private float _radius = 0.05f;
    [SerializeField] private TactSender _tactSender;
    [SerializeField] private GameObject _tactSourcesParent;

    public ShotData Data { get; private set; }
    public string ID => Data.ID;
    public Vector3 SpawnPosition => Data.SpawnPosition;
    public IPlayer Player => Data.Player;
    private const int BotHearingLayerMask = 1 << 28;
    public TeamID TeamID { get; private set; } // if player turns null by disconnect, teamID is maintained

    public TactSender TactSender => _tactSender;

    public event Action Fired;

    private bool _paused;

    private Transform _earTransform;
    private bool _flyBySoundActive;

    private void Start() {
        if (ConfigurationManager.Configuration.EnableHapticHitFeedback)
            _tactSourcesParent.SetActive(true);
    }

    private void OnEnable() {
        Invoke(nameof(ExceedLifetime), _lifeTime);
        if (GameManager.Instance.MatchTimer == null) return;
        GameManager.Instance.MatchTimer.Paused += OnMatchPaused;
        GameManager.Instance.MatchTimer.Resumed += OnMatchResumed;
    }

    private void OnDisable() {
        CancelInvoke(nameof(ExceedLifetime));
        if (GameManager.Instance.MatchTimer == null) return;
        GameManager.Instance.MatchTimer.Paused -= OnMatchPaused;
        GameManager.Instance.MatchTimer.Resumed -= OnMatchResumed;
    }

    private void OnMatchPaused() {
        _paused = true;
        CancelInvoke(nameof(ExceedLifetime));
    }

    private void OnMatchResumed() {
        _paused = false;
        Invoke(nameof(ExceedLifetime), _lifeTime);
    }

    public void Fire(ShotData shotData, float age) {
        Data = shotData;
        TeamID = shotData.Player.TeamID;
        Vector3 currentPosition = SpawnPosition + age * Data.Speed;
        CheckHits(SpawnPosition, currentPosition);
        transform.position = currentPosition;

        if (shotData.Player.IsMe)
            _flyBySoundActive = false;
        else {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (ownPlayer != null) {
                _earTransform = ownPlayer.PlayerAvatar.Targets.Head;
                _flyBySoundActive = true;
            }
        }

        Fired?.Invoke();
    }

    private void Update() {
        if (_paused)
            return;
        Vector3 position = transform.position;
        Vector3 nextPosition = position + Data.Speed * Time.deltaTime;
        CheckHits(position, nextPosition);
        position = nextPosition;
        transform.position = position;
    }

    private void CheckHits(Vector3 from, Vector3 to) {
        Vector3 step = to - from;
        Vector3 currentPosition = transform.position;

        // todo extract fly-by detection into separate script
        if (_flyBySoundActive && _earTransform != null) {
            Vector3 earPosition = _earTransform.position;
            float distance = Vector3.Distance(currentPosition, earPosition);
            if (distance < _flyByThresholdDistance) {
                _flyBy.TriggerFlyBySoundFake(earPosition, currentPosition, Data.Speed);
                _flyBySoundActive = false;
            }
        }

        //used for hearing range of bot
        if (Physics.SphereCast(currentPosition, _radius, step, out RaycastHit hit, step.magnitude,
            BotHearingLayerMask)) {
            hit.collider.gameObject.GetComponent<HearShots>()?.Hear(this);
        }


        //To ensure, the shot detect hits on very very close objects
        Vector3 startPoint = currentPosition - step.normalized * (_radius * 2);
        if (Physics.SphereCast(startPoint, _radius, step, out hit, step.magnitude, _layerMask)) {
            Hit(hit.collider, hit.point, hit.normal);
        }
    }

    private void ExceedLifetime() {
        _shotManager.DestroyShot(ID);
    }

    private void Hit(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal) {
        //Debug.LogError($"Detected hit of projectile {ID} with {hitCollider}");

        var damageDetector = hitCollider.GetComponent<DamageDetectorBase>();
        IPlayer targetPlayer = damageDetector == null ? null : damageDetector.Player;
        if (targetPlayer == Player) return; // no self hits
        if (targetPlayer != null) {
            if (targetPlayer.PlayerHealth.IsActive)
                _hitGameAction.TriggerPlayerHit(this, targetPlayer, hitPoint, damageDetector.DetectorType);
            return;
        }

        var pillarWall = hitCollider.GetComponent<PillarWall>();
        if (pillarWall != null) {
            string wallID = pillarWall.ID;
            _hitGameAction.TriggerWallHit(this, wallID, hitPoint, hitNormal, hitCollider.tag);
        }
        else {
            _hitGameAction.TriggerEnvironmentHit(this, hitPoint, hitNormal, hitCollider.tag);
        }
    }
}