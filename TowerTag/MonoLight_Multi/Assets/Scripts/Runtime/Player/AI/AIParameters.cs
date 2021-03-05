using UnityEngine;

namespace AI {
    /// <summary>
    /// Scriptable Object for storing various Bot behaviour parameters.
    /// </summary>
    [CreateAssetMenu(menuName = "TowerTag/AI Parameters")]
    public class AIParameters : ScriptableObject {
        [Header("Pillar Selection")]
        [SerializeField, Tooltip("If positive, bot tries to approach enemies")]
        private float _enemyAttractiveness;

        [SerializeField, Tooltip("If positive, bot tries to approach the enemy spawn pillars")]
        private float _enemyBaseAttractiveness;

        [Header("On-pillar movement")]
        [SerializeField, Tooltip("Speed with which the bot moves on the platform")]
        private float _movementSpeed;

        [SerializeField, Tooltip("Angular speed at which the bot rotates")]
        private float _rotationSpeed;

        [SerializeField, Tooltip("Min lateral distance to tower")]
        private float _towerHuggingMin;

        [SerializeField, Tooltip("Max lateral distance to tower")]
        private float _towerHuggingMax;

        [Header("Perception")]

        [SerializeField, Tooltip("Time between seeing something and reacting to it")]
        private float _reactionTime;

        [SerializeField, Tooltip("Amount of time a bot memorizes players and shots")]
        private float _memoryTimeSpan;

        [SerializeField, Tooltip("Bot knows about enemies on occupied pillars which are not within line of sight")]
        private bool _recognizeOccupiedEnemyPillars;

        [SerializeField, Tooltip("When selected, the bot can see whether an enemy has low health.")]
        private bool _focusLowHealthEnemy;

        [SerializeField, Tooltip("Threshold for low enemy health")]
        private float _lowHealthThreshold;


        [Header("Aim and shoot")]
        [SerializeField, Tooltip("Min times bot shoots during fight sequence")]
        private int _minShootFrequency;
        [SerializeField, Tooltip("Max times bot shoots during fight sequence")]
        private int _maxShootFrequency;

        [SerializeField, Tooltip("Aim imprecision in radians")]
        private float _aimImprecision;

        [SerializeField, Tooltip("Aim imprecision is reduced by this divisor")]
        private float _aimCorrectionFactor;

        [SerializeField, Tooltip("Head tilt amount in angle degrees")]
        private float _headTiltAmount;

        [SerializeField, Tooltip("Determines how the aim is affected by the gun recoil. In degrees.")]
        private float _recoil;

        [SerializeField, Tooltip("Toggle, whether or not the bot will try to shoot down walls that are in the way.")]
        private bool _shootDownWalls;

        [SerializeField, Tooltip("Bot needs line of sight target a known enemy")]
        private bool _suppressingFire;


        [Header("Probabilities")]
        [SerializeField, Tooltip("Probability of bot shooting during fight")]
        private float _shootChance;

        [SerializeField, Tooltip("Probability of bot teleporting during fight")]
        private float _jumpChance;

        [SerializeField, Tooltip("Probability of bot shooting if being shot at")]
        private float _counterFireChance;

        [SerializeField, Tooltip("Probability to jump when being shot at")]
        private float _fleeChance;

        [SerializeField, Tooltip("Probability to hide behind pillar during fight")]
        private float _hideChance;

        public float EnemyAttractiveness => _enemyAttractiveness;
        public float EnemyBaseAttractiveness => _enemyBaseAttractiveness;
        public float MovementSpeed => _movementSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float TowerHuggingMin => _towerHuggingMin;
        public float TowerHuggingMax => _towerHuggingMax;
        public float ReactionTime => _reactionTime;
        public float MemoryTimeSpan => _memoryTimeSpan;
        public bool SuppressingFire => _suppressingFire;
        public bool ShootDownWalls => _shootDownWalls;
        public bool RecognizeOccupiedPillars => _recognizeOccupiedEnemyPillars;
        public bool FocusLowHealthEnemy => _focusLowHealthEnemy;
        public float LowHealthThreshold => _lowHealthThreshold;
        public int MinShootFrequency => _minShootFrequency;
        public int MaxShootFrequency => _maxShootFrequency;
        public float AimImprecision => _aimImprecision;
        public float AimCorrectionFactor => _aimCorrectionFactor;
        public float HeadTiltAmount => _headTiltAmount;
        public float Recoil => _recoil;
        public float ShootChance => _shootChance;
        public float JumpChance => _jumpChance;
        public float CounterFireChance => _counterFireChance;
        public float FleeUnderFireChance => _fleeChance;
        public float HideChance => _hideChance;
    }
}