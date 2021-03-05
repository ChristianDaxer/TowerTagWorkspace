using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
#if !UNITY_ANDROID
using System.Windows.Forms.VisualStyles;
#endif
using BehaviorDesigner.Runtime;
using JetBrains.Annotations;
using Photon.Pun;
using RotaryHeart.Lib.SerializableDictionary;
using TowerTag;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.UI;
using static AI.BotVoiceCommandsManager;
using Tooltip = UnityEngine.TooltipAttribute;

namespace AI
{
    /// <summary>
    /// Data container class to hold properties related to AI behaviour. Also handles perception (hearing and seeing) and memory of Bot.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public class BotBrain : MonoBehaviour
    {
        public static Action<float> OnCalculatedAverageKDA;
        public static Action<IPlayer, float> OnCalculatedKDA;

        [Header("Difficulty")] [SerializeField]
        private AIParameters _parameters;

        [SerializeField] private BotDifficulty _difficulty;
        [SerializeField] private bool _autoskilledBot;

        [Header("Autoskill Settings")] [SerializeField]
        private BotDifficultyThresholdDictionary Thresholds;

        private IPlayer _player;
        [SerializeField] private ShotManager _shotManager;

        [Header("Bot Transforms")] [SerializeField]
        private Transform _muzzleTransform;

        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _bodyTransform;
        [SerializeField] private Transform _weaponTransform;

        [Header("Bot Behaviour")]
        [SerializeField, UnityEngine.Tooltip("Shots that come within this range can be heard.")]
        private Transform _hearingRadiusTransform;

        [SerializeField, UnityEngine.Tooltip("This layer mask is used to check whether a player is visible")]
        private LayerMask _seeLayerMask;

        [SerializeField,
         UnityEngine.Tooltip("This layer mask is used to check if a pillar can be targeted for claiming.")]
        private LayerMask _claimLayerMask; //should exclude "mirrorOnly"

        [SerializeField, UnityEngine.Tooltip("This animation curve is used for movement on the platform.")]
        private AnimationCurve _animationCurve;

        [SerializeField, UnityEngine.Tooltip("Duration the bot keeps following a voice command")]
        private float _followVoiceCommandDuration;

        [SerializeField,
         UnityEngine.Tooltip("External Behavior Tree for handling Voice Command Behavior, overrides default tree")]
        private ExternalBehavior _voiceCommandBehavior;

        private VoiceCommands _currentCommand;


        [Header("Bot Fixed Parameters")] //These parameters are fixed for all bots.
        [SerializeField, UnityEngine.Tooltip("Height of head when not taking cover")]
        private float _standingHeight;

        [SerializeField, UnityEngine.Tooltip("Base height of head when taking cover")]
        private float _crouchingHeight;

        [SerializeField, UnityEngine.Tooltip("Height max deviation when taking cover")]
        private float _heightRange;

        [SerializeField, UnityEngine.Tooltip("Half field of view in degrees")]
        private float _maximumViewAngle;

        [SerializeField, UnityEngine.Tooltip("Distance range of vision")]
        private float _visualRange;

        [SerializeField, UnityEngine.Tooltip("Scale of hearing range collider")]
        private float _hearingRadius;

        [SerializeField,
         UnityEngine.Tooltip("Bot will not 'hear' shots that come in with an angle below this threshold.")]
        private float _minIncomingSoundAngle;

        [SerializeField,
         UnityEngine.Tooltip("Predicted position if enemy is not in line of sight. Value is added as lateral offset")]
        private float _minSuppressingFireOffset;

        [SerializeField,
         UnityEngine.Tooltip("Predicted position if enemy is not in line of sight. Value is added as lateral offset")]
        private float _maxSuppressingFireOffset;


        [Header("Bot Animator")] [SerializeField]
        private Animator _botAnimator;

        [Header("Bot Difficulty Levels")] [SerializeField]
        private AIParameters _superEasyBot;

        [SerializeField] private AIParameters _easyBot;
        [SerializeField] private AIParameters _mediumBot;
        [SerializeField] private AIParameters _hardBot;
        [SerializeField] private AIParameters _superHardBot;


        [Header("InGame Debug")] [SerializeField]
        private GameObject _debugCanvas;

        [SerializeField] private Text _debugText;
        [SerializeField] private bool _showDebugCanvas;


        // memory of seen players
        private Dictionary<int, KnownPlayer> _knownPlayers = new Dictionary<int, KnownPlayer>();

        // memory of seen shots
        private Dictionary<string, KnownShot> _knownShots = new Dictionary<string, KnownShot>();
        private Shot _recentlyHeardShot;
        private AIInputController _inputController;

        private BehaviorTree _behaviorTree;

        public BotDifficulty Difficulty => _difficulty;
        public AIParameters AIParameters => _parameters;
        public IPlayer Player => _player = _player ?? GetComponentInParent<IPlayer>();
        public IPlayer EnemyPlayer { get; set; }
        public Pillar TargetPillar { get; set; }
        public ShotManager ShotManager => _shotManager;
        public Vector3 BotPosition => _player.ChargePlayer.AnchorTransform.position;
        public Transform BotHead => _headTransform;
        public Transform BotBody => _bodyTransform;
        public Transform BotWeapon => _weaponTransform;
        public Transform MuzzleTransform => _muzzleTransform;
        public List<KnownShot> KnownShots => _knownShots.Values.ToList();
        public AIInputController InputController => _inputController;
        public Shot RecentlyHeardShot => _recentlyHeardShot;
        public Pillar CurrentPillar => _player.CurrentPillar;
        public AnimationCurve AnimationCurve => _animationCurve;
        public LayerMask ClaimLayerMask => _claimLayerMask;
        public float StandingHeight => _standingHeight;
        public float CrouchingHeight => _crouchingHeight;
        public float HeightRange => _heightRange;
        public float MaximumViewAngle => _maximumViewAngle;
        public float VisualRange => _visualRange;
        public float HearingRadius => _hearingRadius;
        public float MinIncomingSoundAngle => _minIncomingSoundAngle;
        public float MinSuppressingFireOffset => _minSuppressingFireOffset;
        public float MaxSuppressingFireOffset => _maxSuppressingFireOffset;
        public Animator BotAnimator => _botAnimator;

        [Serializable]
        public enum BotDifficulty
        {
            VeryEasy = 4,
            Easy = 0,
            Medium = 1,
            Hard = 2,
            VeryHard = 3
        }

        //public AIParameters AIParameters
        //{
        //    get { return _parameters;}
        //    set { _parameters = value;}
        //}

        private struct KnownPlayer
        {
            public IPlayer Player;
            public float LastSeen;

            public Vector3 BodyPosition;
            //public Vector3 HeadPosition;
            //public Vector3 GunPosition;
        }

        public struct KnownShot
        {
            public Shot Shot;
            public float LastSeen;
            public Vector3 Position;
        }

        private void Awake()
        {
            if (_showDebugCanvas && _debugCanvas != null)
                _debugCanvas.SetActive(true);
            _hearingRadiusTransform.localScale = _hearingRadius * Vector3.one;
            _inputController = GetComponent<AIInputController>();
            _behaviorTree = GetComponent<BehaviorTree>();
        }

        private void OnEnable()
        {
            GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
            MatchStats.OnTeamPointAdded += OnTeamPointAdded;
        }

        private void OnDisable()
        {
            GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
            MatchStats.OnTeamPointAdded -= OnTeamPointAdded;
        }

        private void Start()
        {
            SetAIParameters(Player.BotDifficulty);
            Player.PlayerNetworkEventHandler.UpdateAIParameters(_player.BotDifficulty);
            StartCoroutine(Forget());
        }

        private void Update()
        {
            if (_behaviorTree.GetActiveTasks() == null)
                return;
            IEnumerable<string> taskNames = _behaviorTree.GetActiveTasks()
                .Select(t => t.FriendlyName);
#if UNITY_EDITOR
            _debugText.text =
                $"Current States: {string.Join("; ", taskNames)}"
                + Environment.NewLine
                + $"Known Enemies: {_knownPlayers.Count(p => _player.TeamID != p.Value.Player.TeamID)}"
                + Environment.NewLine
                + $"Selected Enemy: {(EnemyPlayer != null ? EnemyPlayer.PlayerName : "None")}";
#endif
        }

        private void OnTeamPointAdded(TeamID team, int points)
        {
            AutoskillBot();
        }

        private void AutoskillBot()
        {
            if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AB", out object autoskillEnabled))
            {
                Debug.LogError("Couldn't get value AB from room options!");
            }
            
            if ((bool) autoskillEnabled && _player != null)
            {
                Dictionary<int, PlayerStats> playerStats = GameManager.Instance.CurrentMatch.Stats.GetPlayerStats();

                float averageKDA = 0f;

                foreach (var stat in playerStats)
                {
                    float playerKDA = ((float)stat.Value.Kills + stat.Value.Assists) / (stat.Value.Deaths == 0 ? 1 : stat.Value.Deaths);

                    averageKDA += playerKDA;
                }

                averageKDA /= playerStats.Count;

                
                PlayerStats stats = playerStats[_player.PlayerID];
                float ownKDA = ((float)stats.Kills + stats.Assists) / (stats.Deaths == 0 ? 1 : stats.Deaths);

                OnCalculatedAverageKDA.Invoke(averageKDA);
                OnCalculatedKDA.Invoke(_player, ownKDA);

                if ((ownKDA - averageKDA) > Thresholds[_difficulty].DecreaseThreshold)
                {
                    DecreaseAIDifficulty((ownKDA - averageKDA));
                }
                else if ((ownKDA - averageKDA) < Thresholds[_difficulty].IncreaseThreshold)
                {
                    IncreaseAIDifficulty((ownKDA - averageKDA));
                }
            }
        }

#region VoiceCommands

        //This region is for handling received voice commands from real players
        public void RunCommand(VoiceCommands command) {
#if UNITY_EDITOR
            Debug.Log("### current command " + command.ToString());
#endif
            if (_currentCommand == command) return; //if command was already called, it will only be executed once

            switch (command) {
                case VoiceCommands.Attack:
#if UNITY_EDITOR
                    Debug.Log("### DO ATTACK");
#endif
                    UseVoiceCommandBehaviour("AttackCommand");
                    break;
                case VoiceCommands.Defend:
#if UNITY_EDITOR
                    Debug.Log("### DO DEFEND");
#endif
                    UseVoiceCommandBehaviour("DefendCommand");
                    break;
                case VoiceCommands.Claim:
#if UNITY_EDITOR
                    Debug.Log("### DO CLAIM");
#endif
                    UseVoiceCommandBehaviour("ClaimCommand");
                    break;
            }

            _currentCommand = command;
        }

        private void UseVoiceCommandBehaviour(string commandVariableName)
        {
            StopCoroutine(UseCommandBehaviourTree(""));
            _behaviorTree.SendEvent("ResetCommands");
            StartCoroutine(UseCommandBehaviourTree(commandVariableName));
        }

        IEnumerator UseCommandBehaviourTree(string commandVariableName)
        {
            if (_behaviorTree.ExternalBehavior == null)
                _behaviorTree.ExternalBehavior = _voiceCommandBehavior;
            SharedBool givenCommand = (SharedBool) _behaviorTree.GetVariable(commandVariableName);
            givenCommand.Value = true;
#if UNITY_EDITOR
            Debug.Log("### SET BEHAVIOR");
#endif
            yield return new WaitForSeconds(_followVoiceCommandDuration);
            givenCommand = (SharedBool) _behaviorTree.GetVariable(commandVariableName);
            givenCommand.Value = false;
            _behaviorTree.ExternalBehavior = null;
        }

#endregion VoiceCommands

        private void OnMatchHasFinishedLoading(IMatch obj)
        {
            EnemyPlayer = null;
            KnownShots.Clear();
            _knownPlayers.Clear();
            _recentlyHeardShot = null;
        }

        private IEnumerator Forget()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(_parameters.MemoryTimeSpan / 2);

                var knownPlayers = new Dictionary<int, KnownPlayer>();
                foreach (KeyValuePair<int, KnownPlayer> player in _knownPlayers)
                {
                    if (Time.time - player.Value.LastSeen < _parameters.MemoryTimeSpan)
                        knownPlayers.Add(player.Key, player.Value);
                }

                _knownPlayers = knownPlayers;

                var knownShots = new Dictionary<string, KnownShot>();
                foreach (KeyValuePair<string, KnownShot> shot in _knownShots)
                {
                    if (Time.time - shot.Value.LastSeen < _parameters.MemoryTimeSpan)
                        knownShots.Add(shot.Key, shot.Value);
                }

                _knownShots = knownShots;
            }
        }

        public void SeePlayer([NotNull] IPlayer player)
        {
            if (player.ChargePlayer == null || player.ChargePlayer.AnchorTransform == null)
            {
                Debug.LogWarning($"Cannot see player {player}, because body position cannot be determined");
                return;
            }

            _knownPlayers[player.PlayerID] = new KnownPlayer
            {
                Player = player,
                LastSeen = Time.time,
                BodyPosition = player.ChargePlayer.AnchorTransform.position
            };
        }

        public void SeeShot(Shot shot)
        {
            _knownShots[shot.ID] = new KnownShot
            {
                Shot = shot,
                LastSeen = Time.time,
                Position = shot.transform.position
            };
        }

        public void HearShot(Shot shot)
        {
            _recentlyHeardShot = shot;
        }

        public void ClearRecentlyHeardShot()
        {
            _recentlyHeardShot = null;
        }

        public IPlayer[] KnownEnemies()
        {
            Dictionary<int, KnownPlayer>.ValueCollection original = _knownPlayers.Values;
            List<IPlayer> players = new List<IPlayer>();

            foreach (var knownPlayer in original)
            {
                if (knownPlayer.Player != null && knownPlayer.Player.GameObject != null && knownPlayer.Player.TeamID != _player.TeamID
                    && knownPlayer.Player.ChargePlayer != null && knownPlayer.Player.ChargePlayer.AnchorTransform != null)
                {
                    PlayersSortedInsert(players, knownPlayer.Player, Score);
                }
            }

            return players.ToArray();
        }

        private int Score(IPlayer enemyA, IPlayer enemyB)
        {
            float distanceA = (enemyA.ChargePlayer.AnchorTransform.position - BotPosition).sqrMagnitude;
            float distanceB = (enemyB.ChargePlayer.AnchorTransform.position - BotPosition).sqrMagnitude;

            if (distanceA > distanceB)
                return 1;
            else if (distanceA < distanceB)
                return -1;
            else
                return 0;
        }

        private void PlayersSortedInsert(List<IPlayer> players, IPlayer value, Comparison<IPlayer> comparison)
        {
            var startIndex = 0;
            var endIndex = players.Count;
            while (endIndex > startIndex)
            {
                var windowSize = endIndex - startIndex;
                var middleIndex = startIndex + (windowSize / 2);
                var compareToResult = comparison(players[middleIndex], value);
                if (compareToResult == 0)
                {
                    players.Insert(middleIndex, value);
                    return;
                }
                else if (compareToResult < 0)
                {
                    startIndex = middleIndex + 1;
                }
                else
                {
                    endIndex = middleIndex;
                }
            }
            players.Insert(startIndex, value);
        }

        /// <summary>
        /// Returns the closest enemy this bot knows about. Can be null.
        /// </summary>
        public IPlayer ClosestEnemy(float maxDistance = Mathf.Infinity)
        {
            float closestEnemyDistance = float.PositiveInfinity;
            IPlayer closest = null;
            foreach (KnownPlayer knownPlayer in _knownPlayers.Values)
            {
                if (knownPlayer.Player.TeamID == _player.TeamID)
                    continue;
                float distance = Vector3.Distance(knownPlayer.BodyPosition, BotPosition);
                if (distance < closestEnemyDistance && distance <= maxDistance)
                {
                    closestEnemyDistance = distance;
                    closest = knownPlayer.Player;
                }
            }

            return closest;
        }

        private RaycastHit[] hitBuffer = new RaycastHit[1];
        private RaycastHit cachedHit = new RaycastHit();
        /// <summary>
        /// Helper function to evaluate whether a certain player is currently visible.
        /// Considers view frustum and ray casting.
        /// </summary>
        public bool PlayerIsVisibleAt(IPlayer player, Vector3 position, out RaycastHit hitInfo)
        {
            hitInfo = cachedHit;
            Vector3 direction = position - BotHead.position;

            if (Vector3.Angle(direction, BotHead.forward) > MaximumViewAngle)
                return false;

            if (Physics.RaycastNonAlloc(BotHead.position, direction, hitBuffer, direction.magnitude, _seeLayerMask) > 0)
            {
                hitInfo = cachedHit = hitBuffer[0];
                var damageDetectorBase = hitInfo.collider.GetComponent<DamageDetectorBase>();
                if (damageDetectorBase != null && damageDetectorBase.Player == player)
                {
                    #if UNITY_EDITOR
                    Debug.DrawRay(BotHead.position, direction, Color.green, 0.1f);
                    #endif
                    return true;
                }

                #if UNITY_EDITOR
                Debug.DrawRay(BotHead.position, direction, Color.red, 0.1f);
                #endif
            }

            #if UNITY_EDITOR
            else Debug.DrawRay(BotHead.position, direction, Color.blue, 0.1f);
            #endif

            return false;
        }

        /// <summary>
        /// returns true if one of the parts of the bot is visible
        /// </summary>
        public bool PlayerIsVisible([NotNull] IPlayer player)
        {
            return PlayerIsVisible(player, out RaycastHit _);
        }

        private Transform[] cachedTargets = new Transform[10];

        /// <summary>
        /// returns true if one of the parts of the bot is visible
        /// </summary>
        public bool PlayerIsVisible([NotNull] IPlayer player, out RaycastHit hitInfo) {
            hitInfo = new RaycastHit();
            // merge prioritized targets with body components

            Transform[] prioritizedTargets = player.PlayerAvatar.Targets.PrioritizedTargets;
            Collider[] gunCollider = player.GunCollider;

            int targetCount = prioritizedTargets.Length + gunCollider.Length;
            if (targetCount > cachedTargets.Length)
                cachedTargets = new Transform[prioritizedTargets.Length];

            Array.Copy(prioritizedTargets, 0, cachedTargets, 0, prioritizedTargets.Length);
            Array.Copy(gunCollider, 0, cachedTargets, prioritizedTargets.Length, gunCollider.Length);

            for (int i = 0; i < targetCount; i++)
                if (PlayerIsVisibleAt(player, cachedTargets[i].position, out hitInfo))
                    return true;

            return false;
        }


        /// <summary>
        /// returns true if pillar of given player is visible to the bot
        /// </summary>
        public bool PlayerPillarIsVisible([NotNull] IPlayer player)
        {
            if (player.CurrentPillar == null)
                return false;

            Vector3 direction = player.CurrentPillar.transform.position + 1.7f * Vector3.up - BotHead.position;
            if (Physics.RaycastNonAlloc(BotHead.position, direction, hitBuffer, direction.magnitude,
                _seeLayerMask) > 0)
            {
                var pillarScript = hitBuffer[0].collider.GetComponentInParent<Pillar>();
                if (pillarScript != null && pillarScript.Owner == player)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// returns true if a shot is visible to the bot
        /// </summary>
        public bool ShotIsVisible(Shot shot)
        {
            Vector3 direction = shot.transform.position - BotHead.position;

            if (Vector3.Angle(direction, BotHead.forward) > MaximumViewAngle)
                return false;

            if (Physics.RaycastNonAlloc(BotHead.position, direction, hitBuffer, direction.magnitude, _seeLayerMask) > 1)
                return false;

            return true;
        }


        /// <summary>
        /// Increases bot difficulty to the next higher difficulty
        /// </summary>
        public void IncreaseAIDifficulty(float avg = 0f)
        {
            int numeralDifficulty = (int) _difficulty;

            if (numeralDifficulty == 4)
            {
                numeralDifficulty = 0;
            }
            else
            {
                numeralDifficulty++;
            }

            if (numeralDifficulty >= 0 && numeralDifficulty <= 3)
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"Autoskill: {_player.PlayerName} from {_difficulty.ToString()} to {((BotDifficulty) numeralDifficulty).ToString()} ({avg} < {Thresholds[_difficulty].DecreaseThreshold})");
#endif

                SetAIParameters((BotDifficulty) numeralDifficulty);
            }
        }

        /// <summary>
        /// Decreases bot difficulty to the next lower difficulty
        /// </summary>
        public void DecreaseAIDifficulty(float avg = 0f)
        {
            int numeralDifficulty = (int) _difficulty;

            if (numeralDifficulty == 0)
            {
                numeralDifficulty = 4;
            }
            else
            {
                numeralDifficulty--;
            }

            if (numeralDifficulty >= 0 && numeralDifficulty <= 4)
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"Autoskill: Decreasing difficulty of {_player.PlayerName} from {_difficulty.ToString()} to {((BotDifficulty) numeralDifficulty).ToString()} ({avg} > {Thresholds[_difficulty].DecreaseThreshold})");
#endif

                SetAIParameters((BotDifficulty) numeralDifficulty);
            }
        }

        /// <summary>
        /// returns true if a shot is visible to the bot
        /// </summary>
        public void SetAIParameters(BotDifficulty difficulty)
        {
            // print($"Parameters set to {difficulty.ToString()}");
            switch (difficulty)
            {
                case BotDifficulty.VeryEasy:
                    _parameters = _superEasyBot;
                    break;

                case BotDifficulty.Easy:
                    _parameters = _easyBot;
                    break;

                case BotDifficulty.Medium:
                    _parameters = _mediumBot;
                    break;

                case BotDifficulty.Hard:
                    _parameters = _hardBot;
                    break;

                case BotDifficulty.VeryHard:
                    _parameters = _superHardBot;
                    break;

                default:
                    _parameters = _easyBot;
                    break;
            }

            BehaviorTree behaviourTree = GetComponent<BehaviorTree>();

            //Debug.Log("#### Behaviour Tree restarted with difficulty " +  difficulty.ToString());
            behaviourTree.DisableBehavior(false); //false means don't pause the tree, since we want to restart it
            behaviourTree.EnableBehavior();
            _difficulty = difficulty;
        }
    }
}