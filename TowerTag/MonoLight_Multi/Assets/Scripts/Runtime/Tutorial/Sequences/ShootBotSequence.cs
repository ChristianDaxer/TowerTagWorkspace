using System.Collections;
using System.Linq;
using AI;
using TowerTag;
using UnityEngine;
using UnityEngine.Audio;

namespace Tutorial {
    [CreateAssetMenu(menuName = "TutorialSequences/ShootBotSequence", fileName = nameof(ShootBotSequence))]
    public class ShootBotSequence : TutorialSequence {
        [SerializeField] private float _timeTillInfoPinSpawns;
        [SerializeField] private float _timeTillVideoExplainerSpawns = 10f;
        [SerializeField] private ToolTipInfoPin _infoPinPrefab;
        [SerializeField] private GameObject _videoPrefab;
        [SerializeField] private string _shootEnemyText = "SHOOT ME";
        [SerializeField] private string _nextPillarText = "MOVE HERE";
        [SerializeField] private bool _canBotShoot;
        [SerializeField] private BotBrain.BotDifficulty _botLevel;
        [SerializeField] private AudioClip _shootBotHint;
        [SerializeField] private AudioMixerGroup _mixer;

        private float _timeInSequence;
        private IPlayer _bot;
        private bool _botDied;
        private ToolTipInfoPin _shootInfoPin;
        private ToolTipInfoPin _moveInfoPin;
        private GameObject _video;
        private bool _hasPlayerTeleported;
        private Coroutine _waitForTeleport;
        private TutorialSequencer _tutorialSequencer;

        protected override void ResetValues() {
            _ownPlayer.TeleportHandler.TeleportRequested -= OnTeleportRequested;
            _moveInfoPin = null;
            _shootInfoPin = null;
            _tutorialSequencer = null;
            _timeInSequence = 0;
            _hasPlayerTeleported = false;
            _botDied = false;
        }

        protected override IEnumerator StartSequence() {
            _tutorialSequencer = FindObjectOfType<TutorialSequencer>();
            _ownPlayer.TeleportHandler.TeleportRequested += OnTeleportRequested;
            
            AddNewBot();
            yield return new WaitForSeconds(0.5f); //TODO: Botmanager sends event when bot ready (2021-01-27, MU)
            SetUpBotForSequence();

            AudioSource src = _tutorialSequencer.gameObject.AddComponent<AudioSource>();
            src.clip = _shootBotHint;
            src.outputAudioMixerGroup = _mixer;
            src.Play();
        }

        private void OnTeleportRequested(Pillar player, Pillar pillar, int time) {
            _hasPlayerTeleported = true;
            if (_moveInfoPin != null)
                Destroy(_moveInfoPin.gameObject);
        }

        private void OnBotDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
            _waitForTeleport = StaticCoroutine.StartStaticCoroutine(WaitForNextTeleport());
        }

        private void AddNewBot() {
            Pillar pillar = _tutorialSequencer.PillarGetNextPillar(_ownPlayer.CurrentPillar);
            
            TeamID enemyTeamID = GetBotTeamID();
            if (pillar != null)
            {
                pillar.OwningTeamID = enemyTeamID;
            }

            _bot = BotManager.Instance.AddBot(enemyTeamID, _botLevel);
        }

        private void SetUpBotForSequence()
        {
            if (!_canBotShoot)
            {
                _bot.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.AliveButDisabled);
            }

            _bot.PlayerHealth.PlayerDied += OnBotDied;
        }

        private TeamID GetBotTeamID()
        {
            if (_ownPlayer.TeamID < 0)
            {
                Debug.LogError("Own Player has no team yet - Bot joins team Neutral.");
                return TeamID.Neutral;
            }
            
            TeamID enemyTeamID = TeamManager.Singleton.GetEnemyTeamIDOfPlayer(_ownPlayer);
            Debug.Log("Bot Team: " + enemyTeamID);
            
            return enemyTeamID;
        }

        public override void Update() {
            if (IsCompleted()) return;

            if (_timeInSequence >= _timeTillInfoPinSpawns && _shootInfoPin == null && _bot != null && _bot.IsAlive) {
                _shootInfoPin =
                    InitializeInfoPin(_bot.PlayerAvatar.Targets.Head, _shootEnemyText, true, true);
            }

            _timeInSequence += Time.deltaTime;
        }

        private IEnumerator WaitForNextTeleport() {
            if (_shootInfoPin != null) {
                _shootInfoPin.EndAnimation();
                yield return new WaitUntil(() => _shootInfoPin.IsAnimatorInDefaultState);
            }
            else {
                yield return new WaitUntil(() => ShotManager.Singleton.Shots.All(shot => shot.Player != _bot));
            }

            var nextPillar = _bot.CurrentPillar;
            DestroyBot();
            _moveInfoPin =
                InitializeInfoPin(nextPillar != null ? nextPillar.transform : null, _nextPillarText, true, true);
            yield return new WaitForSeconds(_timeTillVideoExplainerSpawns);
            _video = InstantiateWrapper.InstantiateWithMessage(_videoPrefab,
                _ownPlayer.CurrentPillar != null ? _ownPlayer.CurrentPillar.transform : null);
        }

        private ToolTipInfoPin InitializeInfoPin(Transform target, string text, bool lookToMainCam,
            bool justRotateAroundY) {
            var infoPin = InstantiateWrapper.InstantiateWithMessage(_infoPinPrefab);
            infoPin.Init(target, text, lookToMainCam, justRotateAroundY);
            return infoPin;
        }

        protected override IEnumerator EndSequence(TutorialSequence nextSequence) {
            if (_waitForTeleport != null)
                StaticCoroutine.StopStaticCoroutine(_waitForTeleport);
            if (_video != null)
                Destroy(_video);
            yield return null;
            if (nextSequence != null) nextSequence.Init();
        }

        private void DestroyBot() {
            _botDied = true;
            _bot.PlayerHealth.PlayerDied -= OnBotDied;
            Destroy(_bot.GameObject.CheckForNull());
            _bot = null;
            _moveInfoPin = null;
            _shootInfoPin = null;
        }

        public override bool IsCompleted() {
            return _botDied && _hasPlayerTeleported;
        }
    }
}