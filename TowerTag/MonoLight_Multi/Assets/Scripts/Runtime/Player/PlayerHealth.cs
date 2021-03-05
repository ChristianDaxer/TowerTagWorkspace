using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using Boo.Lang;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class PlayerHealth : MonoBehaviour {
    #region local Member & Properties

    [SerializeField, Tooltip("The player associated with this component")]
    private IPlayer _player;

    public IPlayer Player {
        get => _player;
        set => _player = value;
    }

    [SerializeField, Tooltip("The health of this player is limited to this amount")]
    private int _maxHealth = 100;

    public int MaxHealth {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    private int _currentHealth;

    public int CurrentHealth {
        get => _currentHealth;
        private set => _currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    public float HealthFraction => (float) CurrentHealth / MaxHealth;

    /// <summary>
    /// IsAlive describes if the Player was set alive or dead in TeamDeathMatch_LMS.
    /// Only used & synced in TeamDeathMatch_LMS so far. So be careful and double check if u want to use it otherwise!
    /// </summary>
    public bool IsAlive => CurrentHealth > 0;

    public bool IsAtFullHealth => CurrentHealth >= MaxHealth;

    [SerializeField, Tooltip("Collider that accept damage to this player health")]
    private DamageDetectorBase[] _collisionDetectors;

    public DamageDetectorBase[] CollisionDetectors {
        get => _collisionDetectors;
        set => _collisionDetectors = value;
    }


    //Health reduction in Tower
    private float _startDelay = 0.5f;
    private float _tickInterval = 0.5f;
    private int _dmgPerTick = 7;

    #endregion

    #region events

    [FormerlySerializedAs("_healthChanged")] [SerializeField, Tooltip("Player health changed event")]
    private SharedPlayerHealthChange _sharedHealthChanged;

    public delegate void DamagedListener(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage);

    public event DamagedListener TookDamage;

    public delegate void DiedListener(PlayerHealth playerHealth, [CanBeNull] IPlayer enemyWhoAppliedDamage,
        byte colliderType);

    public event DiedListener PlayerDied;

    public delegate void HealthChangedListener(PlayerHealth playerHealth, int newHealth, IPlayer other,
        byte colliderType);

    public event HealthChangedListener HealthChanged;

    public delegate void PlayerRevivedListener(IPlayer player);

    public event PlayerRevivedListener PlayerRevived;

    #endregion

    #region Member only living on Master

    // holds all enemy players who applied damage to this player since his last death.
    // maintained only on master
    private readonly List<int> _enemiesWhoAppliedDamageOnMasterOnly = new List<int>();
    private Coroutine _healthLossOverTime;
    public int[] EnemiesWhoAppliedDamageOnMasterOnly => _enemiesWhoAppliedDamageOnMasterOnly.ToArray();

    #endregion

    #region Init & DamageHandling

    private void Awake() {
        foreach (DamageDetectorBase detector in _collisionDetectors) {
            if (_player != null)
                detector.Init(_player);
        }
    }

    public void RegisterEvents(IHealthVisuals visuals) {
        if (visuals != null) {
            HealthChanged += visuals.OnHealthChanged;
            TookDamage += visuals.OnTookDamage;
            PlayerDied += visuals.OnPlayerDied;
        }

        _player.InTowerStateChanged += OnInTowerStateChanged;
    }

    private void OnInTowerStateChanged(IPlayer player, bool inTower) {
        if (inTower)
            _healthLossOverTime = StartCoroutine(LoseHealthOverTime());
        else {
            if (_healthLossOverTime != null)
                StopCoroutine(_healthLossOverTime);
        }
    }

    private IEnumerator LoseHealthOverTime() {
        yield return new WaitForSeconds(_startDelay);
        while (true) {
            if (GameManager.Instance.CurrentMatch != null && GameManager.Instance.CurrentMatch.IsActive
                || TTSceneManager.Instance.IsInHubScene
                || TTSceneManager.Instance.IsInTutorialScene) {
                if(PhotonNetwork.IsMasterClient)
                    TakeDamage(_dmgPerTick, DamageDetectorBase.ColliderType.Body);
                else if(_player.IsMe && _player.IsAlive)
                {
                    HitGameAction.Instance.InvokePlayerGotHitEvent(_player as Player);
                    TookDamage?.Invoke(this, _player);
                }
            }

            yield return new WaitForSeconds(_tickInterval);
        }

        _healthLossOverTime = null;
    }

    /// <summary>
    /// Take damage respecting the damage-detector modificator. Raise the respective events.
    /// Player dies if health drops to 0.
    /// </summary>
    /// <param name="detectorType">Type of the target that registered the damage</param>
    /// <param name="damage">The taken damage</param>
    /// <param name="damageDealer">The player that dealt the damage</param>
    public void TakeDamage(int damage, DamageDetectorBase.ColliderType detectorType, IPlayer damageDealer = null) {
        if (!IsActive || !IsAlive)
            return;

        //  apply damage
        int clampedDamage = Mathf.Clamp(damage, 0, CurrentHealth);
        CurrentHealth -= clampedDamage;

        // register damage dealer
        if (PhotonNetwork.IsMasterClient && clampedDamage > 0
                                         && _enemiesWhoAppliedDamageOnMasterOnly != null
                                         && damageDealer != null) {
            if (!_enemiesWhoAppliedDamageOnMasterOnly.Contains(damageDealer.PlayerID)) {
                _enemiesWhoAppliedDamageOnMasterOnly.Add(damageDealer.PlayerID);
            }
        }

        // raise events
        TookDamage?.Invoke(this, damageDealer);

        HealthChanged?.Invoke(this, CurrentHealth, damageDealer, (byte) detectorType);
        if (_sharedHealthChanged != null) {
            _sharedHealthChanged.Set(this, new HealthChange(
                -clampedDamage,
                CurrentHealth,
                _player,
                damageDealer,
                detectorType));
        }

        // maybe die
        if (CurrentHealth <= 0) {
            Die(damageDealer, (byte) detectorType);
        }
    }

    /// <summary>
    /// Handle a health change that was triggered remotely.
    /// </summary>
    /// <param name="newHealthValue">The new health of this player</param>
    /// <param name="enemyWhoAppliedDamage">The enemy that applied the damage</param>
    /// <param name="colliderType">The type of the collider that registered the damage</param>
    public void OnHealthChangedRemote(int newHealthValue, [CanBeNull] IPlayer enemyWhoAppliedDamage, byte colliderType) {
        int healthChangeAmount = newHealthValue - CurrentHealth;

        CurrentHealth = newHealthValue;

        // raise events
        HealthChanged?.Invoke(this, CurrentHealth, enemyWhoAppliedDamage, colliderType);
        if (_sharedHealthChanged != null) {
            _sharedHealthChanged.Set(this, new HealthChange(
                healthChangeAmount,
                CurrentHealth,
                _player,
                enemyWhoAppliedDamage,
                (DamageDetectorBase.ColliderType) colliderType));
        }

        // maybe die
        if (CurrentHealth <= 0) {
            Die(enemyWhoAppliedDamage, colliderType);
        }
    }

    public void OnPlayerRevivedRemote() {
        PlayerRevived?.Invoke(Player);
    }

    /// <summary>
    /// Heal this player by a given amount.
    /// </summary>
    public void AddHealth(int baseHealingAmount, [NotNull] IPlayer healingPlayer) {
        int healingAmount = Mathf.Clamp(baseHealingAmount, 0, MaxHealth - CurrentHealth);
        if (healingAmount == 0) return;
        bool reviving = !IsAlive;
        // apply healing
        CurrentHealth += healingAmount;

        if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null
                                         && GameManager.Instance.CurrentMatch.IsActive)
            GameManager.Instance.CurrentMatch.Stats.AddHeal(Player, healingPlayer, healingAmount);

        // raise events
        HealthChanged?.Invoke(this, CurrentHealth, healingPlayer, 0);
        if (_sharedHealthChanged != null)
            _sharedHealthChanged.Set(this, new HealthChange(healingAmount, CurrentHealth, _player, healingPlayer));
        if (reviving)
            PlayerRevived?.Invoke(Player);
    }

    /// <summary>
    /// Restore player health to maximum health. Fires respective events.
    /// </summary>
    public void RestoreMaxHealth() {
        int healingAmount = MaxHealth - CurrentHealth;
        bool reviving = !IsAlive;
        CurrentHealth = MaxHealth;

        // raise events

        HealthChanged?.Invoke(this, CurrentHealth, null, 0);
        if (_sharedHealthChanged != null)
            _sharedHealthChanged.Set(this, new HealthChange(healingAmount, CurrentHealth, _player));
        if (reviving)
            PlayerRevived?.Invoke(Player);
    }

    /// <summary>
    /// The Player's health dropped to zero. Handle his death. Fill out all the forms, call the morgue.
    /// </summary>
    /// <param name="enemyWhoAppliedDamage">The player that ultimately ended things for this player</param>
    /// <param name="colliderType">The type of the collider that registered the very last hit</param>
    private void Die(IPlayer enemyWhoAppliedDamage, byte colliderType) {
        // raise events
        PlayerDied?.Invoke(this, enemyWhoAppliedDamage, colliderType);

        // clear list of damage dealers
        _enemiesWhoAppliedDamageOnMasterOnly?.Clear();
    }

    public void KillPlayerFromMaster() {
        CurrentHealth = 0;
        HealthChanged?.Invoke(this, 0, null, 0);
        if (_sharedHealthChanged != null) {
            _sharedHealthChanged.Set(this, new HealthChange(
                -MaxHealth,
                CurrentHealth,
                _player));
        }

        Die(null, 0);
    }

    #endregion

    #region Activate

    /// <summary>
    /// To access the current status is the Player is alive or not
    /// </summary>
    public bool IsActive { get; private set; } = true;

    public SharedPlayerHealthChange SharedHealthChanged => _sharedHealthChanged;

    public void OnSetActive(bool setActive) {
        IsActive = setActive;
    }

    #endregion
}