public struct PlayerState {
    private byte _state;

    // DamageModel disabled: index 0 in bitMask
    public bool IsImmortal {
        get => BitOperations.CheckBitInMask(_state, 0);
        private set => _state = BitOperations.SetBitInMask(_state, 0, value);
    }

    // Gun disabled: index 1 in bitMask
    public bool IsGunDisabled {
        get => BitOperations.CheckBitInMask(_state, 1);
        private set => _state = BitOperations.SetBitInMask(_state, 1, value);
    }

    // LimboMode enabled: index 2 in bitMask
    public bool IsInLimbo {
        get => BitOperations.CheckBitInMask(_state, 2);
        private set => _state = BitOperations.SetBitInMask(_state, 2, value);
    }

    // isCollidingWithPillar is handled special because it should be set only locally (not transmitted over network)
    public bool IsCollidingWithPillar { get; set; }

    public bool IsGunInPillar { private get; set; }

    // has the player left the bounding area of the Chaperone (our Chaperone not the SteamVr one)

    public bool PlayerLeftChaperoneBounds { private get; set; }
    public static PlayerState Alive => new PlayerState(false, false, false);
    public static PlayerState AliveButDisabled => new PlayerState(false, true, false);

    public static PlayerState Dead => new PlayerState(true, true, false);
    public static PlayerState DeadButNoLimbo => new PlayerState(true, true, false);

    public bool IsAlive => !IsImmortal && !IsGunDisabled && !IsInLimbo;
    public bool IsDead => IsImmortal && IsGunDisabled && !IsInLimbo;
    public bool IsAliveButDisabled => !IsImmortal && IsGunDisabled && !IsInLimbo;
    public bool IsDeadButNoLimbo => IsImmortal && IsGunDisabled && !IsInLimbo;

    // constructor for local (convenience) creation
    public PlayerState(bool isImmortal, bool isGunDisabled, bool isInLimbo) {
        _state = 0;
        IsCollidingWithPillar = false;
        PlayerLeftChaperoneBounds = false;
        IsGunInPillar = false;

        IsImmortal = isImmortal;
        IsGunDisabled = isGunDisabled;
        IsInLimbo = isInLimbo;
    }

    // Setter for local (convenience) state definition
    public void Set(PlayerState stateToCopy, bool copyIsCollidingWithPillar, bool copyPlayerLeftChaperoneBounds,
        bool copyGunIsCollidingWithPillar) {
        IsImmortal = stateToCopy.IsImmortal;
        IsGunDisabled = stateToCopy.IsGunDisabled;
        IsInLimbo = stateToCopy.IsInLimbo;

        if (copyIsCollidingWithPillar)
            IsCollidingWithPillar = stateToCopy.IsCollidingWithPillar;

        if (copyPlayerLeftChaperoneBounds)
            PlayerLeftChaperoneBounds = stateToCopy.PlayerLeftChaperoneBounds;

        if (copyGunIsCollidingWithPillar)
            IsGunInPillar = stateToCopy.IsGunInPillar;
    }

    /// set State to default (all disabled and not colliding (if resetColliding is true))
    public void Reset(bool resetCollidingWithPillar, bool resetPlayerLeftChaperoneBounds, bool resetGunInPillar) {
        _state = 0;

        if (resetCollidingWithPillar)
            IsCollidingWithPillar = false;

        if (resetPlayerLeftChaperoneBounds)
            PlayerLeftChaperoneBounds = false;

        if (resetGunInPillar)
            IsGunInPillar = false;
    }

    public void Serialize(BitSerializer stream) {
        if (stream.IsWriting) {
            stream.WriteByte(_state, 3);
        }
        else {
            _state = stream.ReadByte(3);
        }
    }

    public bool ShouldGunControllerBeDisabled() {
        return IsCollidingWithPillar || IsGunDisabled || PlayerLeftChaperoneBounds || IsGunInPillar;
    }
}