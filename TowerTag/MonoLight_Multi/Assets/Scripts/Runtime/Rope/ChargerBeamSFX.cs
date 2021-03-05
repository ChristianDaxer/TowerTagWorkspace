using TowerTag;
using UnityEngine;

public class ChargerBeamSFX : MonoBehaviour {
// looped Sounds
    private AudioSource _tensionSource;
    private AudioSource _shotSource;

    [SerializeField] private string _tensionSoundName;
    private Sound _tensionSound;
    private AudioSource _chargeSource;

    [SerializeField] private string _chargeSoundNamePillar;
    private Sound _chargeSoundPillar;

    [SerializeField] private string _chargeSoundNamePlayer;
    private Sound _chargeSoundPlayer;

// short Sounds
    private AudioSource _shortSoundsSource;

    [SerializeField] private string _shotSoundName;
    private Sound _shotSound;
    [SerializeField] private string _abortChargeSoundName;
    private Sound _abortChargeSound;
    [SerializeField] private string _finishedChargeSoundName;
    private Sound _finishedChargeSound;
    [SerializeField] private string _hitSoundNamePlayer;
    private Sound _hitSoundPlayer;
    [SerializeField] private string _hitSoundNamePillar;
    private Sound _hitSoundPillar;

    private IPlayer _owner;

    private IChargerBeamRenderer _chargerBeam;
    private Chargeable _currentTarget;
    private bool _isCharging;

    [SerializeField] private RopeGameAction _ropeGameAction;

    private Sound _currentChargeSound;
    private Sound _currentHitSound;

    private void OnEnable() {
        _ropeGameAction.RopeConnectedToChargeable += OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting += OnDisconnecting;
        _ropeGameAction.AttachFailed += OnAttachFailed;
    }

    private void OnDisable() {
        _ropeGameAction.RopeConnectedToChargeable -= OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting -= OnDisconnecting;
        _ropeGameAction.AttachFailed -= OnAttachFailed;
    }

    private void Update() {
        if (!_isCharging) return;
        UpdateCharge(_currentTarget.CurrentCharge.value);
    }

    private void OnRopeConnectedToChargeable(RopeGameAction sender, IPlayer player, Chargeable target) {
        if (player != _owner) return;
        StartConnect(target);
    }

    private void OnAttachFailed(RopeGameAction sender, IPlayer player, Chargeable chargeable) {
        if (player != _owner) return;
        StartConnect(chargeable);
        Disconnect(false);
    }

    private void OnDisconnecting(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose) {
        if (player != _owner) return;
        Disconnect(onPurpose);
    }

    public void Init(IChargerBeamRenderer chargerBeam, IPlayer owner) {
        _owner = owner;
        _chargerBeam = chargerBeam;
        _isCharging = false;

        if (_tensionSource == null)
            _tensionSource = gameObject.AddComponent<AudioSource>();

        if (_chargeSource == null)
            _chargeSource = gameObject.AddComponent<AudioSource>();

        if (_shortSoundsSource == null)
            _shortSoundsSource = gameObject.AddComponent<AudioSource>();

        if (_shotSource == null)
            _shotSource = gameObject.AddComponent<AudioSource>();

        chargerBeam.RolledOut += FinishedConnect;
        chargerBeam.TensionValueChanged += UpdateTension;
        chargerBeam.TeleportTriggered += OnTeleportTriggerChangedListener;
        _owner.GunController.DetachedAccidentally += OnDetachedAccidentally;
        GetSoundsFromDatabase();
    }

    private void OnDetachedAccidentally(GunController gunController) {
        if (_owner.IsMe) PlayErrorSound();
    }


    private void GetSoundsFromDatabase() {
        SoundDatabase soundDatabase = SoundDatabase.Instance;

        _tensionSound = soundDatabase.GetSound(_tensionSoundName);
        _chargeSoundPillar = soundDatabase.GetSound(_chargeSoundNamePillar);
        _chargeSoundPlayer = soundDatabase.GetSound(_chargeSoundNamePlayer);

        _shotSound = soundDatabase.GetSound(_shotSoundName);
        _abortChargeSound = soundDatabase.GetSound(_abortChargeSoundName);
        _finishedChargeSound = soundDatabase.GetSound(_finishedChargeSoundName);
        _hitSoundPlayer = soundDatabase.GetSound(_hitSoundNamePlayer);
        _hitSoundPillar = soundDatabase.GetSound(_hitSoundNamePillar);
    }

    private void StartConnect(Chargeable target) {
        _currentTarget = target;
        _isCharging = true;
        if (target is Pillar pillar) {
            pillar.OwningTeamChanged += OnPillarOwningTeamChanged;
        }

        if (target is OptionSelector optionSelector) {
            optionSelector.OnOptionClaimed.AddListener(OnOptionClaimed);
        }

        if (target is ChargePlayer player) {
            player.Owner.PlayerHealth.HealthChanged += OnPlayerHealthChanged;
        }

        if (_shortSoundsSource.isPlaying)
            _shortSoundsSource.Stop();

        if (_tensionSource.isPlaying)
            _tensionSource.Stop();

        if (_chargeSource.isPlaying)
            _chargeSource.Stop();


        // Play Shot / Muzzle-flash sound
        _shotSound.InitSource(_shotSource);
        _shotSource.Play();

        // reset loop sources
        _tensionSound.InitSource(_tensionSource);

        if (_currentTarget.ChargeableType == ChargeableType.Pillar) {
            _currentChargeSound = _chargeSoundPillar;
        }
        else if (_currentTarget.ChargeableType == ChargeableType.Player) {
            _currentChargeSound = _chargeSoundPlayer;
        }
        else {
            // others Upgrades etc...
            _currentChargeSound = _chargeSoundPillar;
        }

        _currentChargeSound.InitSource(_chargeSource);
    }

    private void FinishedConnect() {
        if (_shortSoundsSource.isPlaying)
            _shortSoundsSource.Stop();

        if (_currentTarget == null) return;

        // Play HitSound for target (Player/Pillar...)
        if (_currentTarget.ChargeableType == ChargeableType.Pillar) {
            //_hitSoundPillar.InitSource(_shortSoundsSource);
            _currentHitSound = _hitSoundPillar;
        }
        else if (_currentTarget.ChargeableType == ChargeableType.Player) {
            //_hitSoundPlayer.InitSource(_shortSoundsSource);
            _currentHitSound = _hitSoundPlayer;
        }
        else {
            // others Upgrades etc...
            _currentHitSound = _hitSoundPillar;
        }

        _currentHitSound.InitSource(_shortSoundsSource);

        _shortSoundsSource.Play();

        // Play tension Sound
        _tensionSource.pitch = _tensionSound.GetPitch(_chargerBeam.Tension);

        // Play charge sound
        _chargeSource.pitch = _currentChargeSound.GetPitch(_currentTarget.CurrentCharge.value);

        if (_currentTarget.CanCharge(_owner)) {
            _chargeSource.Play();
        }
        else {
            _tensionSource.Play();
        }
    }

    private void Disconnect(bool onPurpose) {
        _isCharging = false;
        if (!onPurpose || _currentTarget is Claimable claimable && _owner != null
                                                                && claimable.OwningTeamID != _owner.TeamID) {
            if (_owner.IsMe) PlayErrorSound();
        }

        if (_currentTarget is Pillar pillar) {
            pillar.OwningTeamChanged -= OnPillarOwningTeamChanged;
        }

        if (_currentTarget is OptionSelector optionSelector) {
            optionSelector.OnOptionClaimed.RemoveListener(OnOptionClaimed);
        }

        if (_currentTarget is ChargePlayer player) {
            player.Owner.PlayerHealth.HealthChanged -= OnPlayerHealthChanged;
        }

        _currentTarget = null;
        _tensionSource.Stop();
        _chargeSource.Stop();
    }

    private void OnPlayerHealthChanged(PlayerHealth playerHealth, int newHealth, IPlayer other, byte colliderType) {
        if (playerHealth.IsAtFullHealth) OnChargingFinished();
    }

    private void OnOptionClaimed(IPlayer player) {
        OnChargingFinished();
    }

    private void OnPillarOwningTeamChanged(Claimable claimable, TeamID oldTeamID, TeamID newTeamID, IPlayer[] attachedPlayers) {
        OnChargingFinished();
        claimable.OwningTeamChanged -= OnPillarOwningTeamChanged;
    }

    private void OnChargingFinished() {
        _chargeSource.Stop();
        _tensionSource.Play();

        // play finished claim source sound
        if (_shortSoundsSource.isPlaying)
            _shortSoundsSource.Stop();

        _finishedChargeSound.InitSource(_shortSoundsSource);
        _shortSoundsSource.Play();
    }

    private void UpdateCharge(float currentCharge) {
        _chargeSource.pitch = currentCharge;
    }

    private void UpdateTension(float currentTension) {
        _tensionSource.pitch = _tensionSound.GetPitch(currentTension);
    }


    private void OnTeleportTriggerChangedListener() {
        // play abort sound if not fully charged
        if (_owner.IsMe && _currentTarget is Claimable claimable && claimable.OwningTeamID != _owner.TeamID) {
            PlayErrorSound();
        }
    }

    private void PlayErrorSound() {
        if (_shortSoundsSource.isPlaying)
            _shortSoundsSource.Stop();

        // Play AbortChargeSound
        _abortChargeSound.InitSource(_shortSoundsSource);
        _shortSoundsSource.Play();
    }

    private void OnDestroy() {
        if (_owner != null && _owner.GunController != null)
            _owner.GunController.DetachedAccidentally -= OnDetachedAccidentally;

        if (_chargerBeam != null) {
            _chargerBeam.RolledOut -= FinishedConnect;
            _chargerBeam.TensionValueChanged -= UpdateTension;
            _chargerBeam.TeleportTriggered -= OnTeleportTriggerChangedListener;
        }

        if (_tensionSource != null) {
            if (_tensionSource.isPlaying)
                _tensionSource.Stop();

            Destroy(_tensionSource);
            _tensionSource = null;
        }

        if (_shotSource != null) {
            if (_shotSource.isPlaying)
                _shotSource.Stop();

            Destroy(_shotSource);
            _shotSource = null;
        }

        _tensionSoundName = null;
        _tensionSound = null;

        if (_chargeSource != null) {
            if (_chargeSource.isPlaying)
                _chargeSource.Stop();

            Destroy(_chargeSource);
            _chargeSource = null;
        }

        _chargeSoundNamePillar = null;
        _chargeSoundPillar = null;
        _chargeSoundNamePlayer = null;
        _chargeSoundPlayer = null;

        if (_shortSoundsSource != null) {
            if (_shortSoundsSource.isPlaying)
                _shortSoundsSource.Stop();

            Destroy(_shortSoundsSource);
            _shortSoundsSource = null;
        }

        _shotSoundName = null;
        _shotSound = null;
        _abortChargeSoundName = null;
        _abortChargeSound = null;
        _finishedChargeSoundName = null;
        _finishedChargeSound = null;
        _hitSoundNamePlayer = null;
        _hitSoundPlayer = null;
        _hitSoundNamePillar = null;
        _hitSoundPillar = null;

        _owner = null;
        _chargerBeam = null;
        _currentTarget = null;

        _currentChargeSound = null;
        _currentHitSound = null;
    }
}