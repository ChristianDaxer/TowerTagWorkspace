using System;
using System.Collections;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

namespace TowerTag
{
    [Serializable]
    public abstract class Match : IMatch, IDisposable
    {
        protected IPhotonService PhotonService { get; }

        /// <summary>
        /// The type of a countdown. Countdowns can occur at the start of the match or round, as well as after a pause.
        /// </summary>
        public enum CountdownType
        {
            StartMatch = 0,
            StartRound = 1,
            ResumeMatch = 2
        }

        #region Properties

        public int MatchID => MatchDescription.MatchID;

        public bool IsLoaded { get; set; }

        public string Scene => MatchDescription.SceneName;

        public abstract MatchStats Stats { get; }

        protected abstract bool CountGoalPillars { get; }

        public abstract GameMode GameMode { get; }

        public bool IsActive => _isActive;

        public int MatchTimeInSeconds
        {
            get => _matchTimeInSeconds;
            protected set => _matchTimeInSeconds = value;
        }

        public int MatchStartAtTimestamp => _matchStartAtTimestamp;

        public int MatchFinishedAtTimestamp => _matchFinishedAtTimestamp;

        public int RoundStartAtTimestamp => _roundStartAtTimestamp;

        public int RoundFinishedAtTimestamp => _roundFinishedAtTimestamp;
        public int RoundsStarted => Stats.RoundsStarted;
        public bool Paused { get; private set; }

        /// <summary>
        /// List of registered player for this match.
        /// </summary>
        protected void GetPlayers (out IPlayer[] players, out int count) => PlayerManager.Instance.GetParticipatingPlayers(out players, out count);

        protected int GetPlayersCount () => PlayerManager.Instance.GetParticipatingPlayersCount();

        /// <summary>
        /// List of Players that are not actively participating.
        /// </summary>
        private void GetSpectators(out IPlayer[] players, out int count) => PlayerManager.Instance.GetSpectatingPlayers(out players, out count);

        /// <summary>
        /// Cached Pillars we registered at the start of the Match.
        /// </summary>
        protected Pillar[] ScenePillars { get; set; }

        public MatchDescription MatchDescription { get; }
        public bool MatchStarted { get; set; }

        #endregion

        #region Members

        private bool IsMatchFull =>
            PlayerManager.Instance.GetParticipatingPlayersCount() > MatchDescription.MatchUp.MaxPlayers;

        private bool _isActive;
        private int _matchTimeInSeconds;
        private int _matchStartAtTimestamp;
        private int _matchFinishedAtTimestamp;
        private int _roundStartAtTimestamp;
        private float _roundStartTime;
        private int _roundFinishedAtTimestamp;
        private TeamID _lastRoundWonByTeamID;
        private bool _stopped;
        private bool _masterClientsSwitched;

        #endregion

        #region Events

        public event MatchAction Initialized;
        public event MatchTimeAction StartingAt;
        public event MatchAction Started;
        public event MatchAction Finished;
        public event MatchAction Stopped;
        public event MatchTimeAction RoundStartingAt;
        public event MatchAction RoundStarted;
        public event RoundFinishAction RoundFinished;
        public event StatsAction StatsChanged;

        #endregion

        #region Core

        protected Match(MatchDescription matchDescription, IPhotonService photonService)
        {
            PhotonService = photonService;
            MatchDescription = matchDescription;
            MatchTimeInSeconds =
                PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.MatchDurationInMinutes)
                    ? (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.MatchDurationInMinutes] * 60
                    : BalancingConfiguration.Singleton.MatchTimeInSeconds;
            _lastRoundWonByTeamID = TeamID.Neutral;
        }

        public virtual void InitMatchOnMaster()
        {
            if (!PhotonService.IsMasterClient) return;
            GameManager.Instance.GameManagerAddedPlayer += OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
            GetPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                OnPlayerAdded(players[i]);

            // test
            var player = PlayerManager.Instance.GetOwnPlayer();

            if (player != null)
            {
                if (player.PlayerStateHandler.PlayerState.Equals(PlayerState.Dead) ||
                    player.PlayerStateHandler.PlayerState.Equals(PlayerState.AliveButDisabled) ||
                    player.PlayerStateHandler.PlayerState.Equals(PlayerState.DeadButNoLimbo))
                {
                    RespawnPlayer(player);
                }
            }

            InitPillars();
            if (!_masterClientsSwitched) InitStats();
            CalculateNumberOfPillarsOwnedByTeams();
            Initialized?.Invoke(this);
            _masterClientsSwitched = false;
        }


        /// <summary>
        /// Initialize <see cref="MatchStats"/>.
        /// </summary>
        protected abstract void InitStats();

        /// <summary>
        /// Initialize Pillars & register callbacks.
        /// </summary>
        protected abstract void InitPillars();

        public void StartMatchAt(int startTimestamp, int finishTimestamp)
        {
            Debug.Log($"Scheduled match start for timestamp {startTimestamp}");

            _matchStartAtTimestamp = startTimestamp;
            _matchFinishedAtTimestamp = finishTimestamp;

            // before a new Round is defined we set the complete matchTime as start/end-timestamps for first Round
            _roundStartAtTimestamp = startTimestamp;
            _roundFinishedAtTimestamp = finishTimestamp;
            if (PhotonService.IsMasterClient)
            {
                GetPlayers(out var players, out var count);
                for (int i = 0; i < count; i++)
                {
                    PrepareRespawn(players[i], startTimestamp, CountdownType.StartMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                }

                GetSpectators(out var spectators, out var spectatorCount);
                for (int i = 0; i < spectatorCount; i++)
                {
                    TeleportPlayerToSpectatorPillarAsSpectator(spectators[i], startTimestamp, CountdownType.StartMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                }
            }

            StartingAt?.Invoke(this, startTimestamp);
        }

        public virtual void StartNewRoundAt(int startTimestamp, int finishTimestamp)
        {
            Debug.Log($"Scheduled new round for timestamp {startTimestamp}");

            _roundStartAtTimestamp = startTimestamp;
            _roundFinishedAtTimestamp = finishTimestamp;

            RoundStartingAt?.Invoke(this, startTimestamp);

            if (PhotonService.IsMasterClient)
            {
                // detach players
                GetPlayers(out var players, out var count);
                for (int i = 0; i < count; i++)
                    if (players[i].AttachedTo != null) players[i].AttachedTo.Detach(players[i]);

                // free spawn pillars | Expensive!
                PillarManager.Instance.GetAllPillars()
                    .Where(pillar => pillar.IsSpawnPillar || pillar.IsGoalPillar)
                    .Apply(PillarManager.ResetPillarOwningTeam)
                    .Where(pillar =>
                        pillar.IsOccupied && pillar.Owner.TeamID != pillar.OwningTeamID) // occupied by enemy
                    .ForEach(pillar => pillar.Owner = null); // free without teleporting
                CalculateNumberOfPillarsOwnedByTeams();

                for (int i = 0; i < count; i++)
                    PrepareRespawn(players[i], startTimestamp, CountdownType.StartRound,
                        TeleportHelper.TeleportDurationType.Respawn);
            }
        }


        public void Pause()
        {
            Paused = true;
        }

        public void Resume()
        {
            GetPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                players[i].PlayerStateHandler.SetPlayerStateOnMaster(
                    players[i].PlayerHealth.IsAlive ? PlayerState.Alive : PlayerState.Dead);
            }

            Paused = false;
        }

        public void StopMatch()
        {
            if (_stopped)
                return;

            Debug.Log("Stopping Match");
            ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;
            _stopped = true;
            _isActive = false;
            IsLoaded = false;
            MatchStarted = false;
            Cleanup();
            Stopped?.Invoke(this);
        }

        public virtual void StartMatch()
        {
            if (PhotonService.IsMasterClient)
            {
                _isActive = true;
                Stats.StartTime = DateTime.Now;
            }

            ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;
#if UNITY_EDITOR
            Debug.Log("Starting match");
#endif
            Started?.Invoke(this);
            _stopped = false;
            MatchStarted = true;
            StartNewRound();
            GameManager.Instance.TriggerMatchStartedEvent(this);
        }

        public virtual void StartNewRound()
        {
            if (PhotonService.IsMasterClient)
            {
                Stats.RoundsStarted++;
                _isActive = true;
                _roundStartTime = GameManager.Instance.MatchTimer.GetCurrentTimerInSeconds();
                GetPlayers(out var players, out var count);
                for (int i = 0; i < count; i++)
                {
                    TeleportHelper.TeleportPlayerOnSpawnPillar(players[i], TeleportHelper.TeleportDurationType.Immediate);
                    RespawnPlayer(players[i]);
                }
            }
            else
            {
                IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
                if (ownPlayer != null)
                    ownPlayer.GunController.CurrentEnergy = 1;
            }

#if UNITY_EDITOR
            Debug.Log("Starting new round");
#endif
            RoundStarted?.Invoke(this);
        }

        protected virtual void FinishRoundOnMaster(TeamID roundWinningTeamID)
        {
            if (!PhotonService.IsMasterClient)
                return;

            Debug.Log("Finishing round");
            _lastRoundWonByTeamID = roundWinningTeamID;
            Stats.AddTeamPoint(roundWinningTeamID);
            Stats.AddRound(new MatchStats.RoundStats
            {
                WinningTeamID = roundWinningTeamID,
                PlayTimeInSeconds = _isActive
                    ? (int) (_roundStartTime - GameManager.Instance.MatchTimer.GetCurrentTimerInSeconds())
                    : 0
            });

            GetPlayers(out var players, out var count);

            for (int i = 0; i < count; i++)
                if (players[i].GameObject != null)
                    Stats.SetPlayTime(players[i], players[i].GameObject.GetComponent<PlayTimeTracker>().PlayTime);

            _isActive = false;
            OnGameStatsChanged();

            RoundFinished?.Invoke(this, roundWinningTeamID);
        }

        public void FinishRoundOnClients()
        {
            if (PhotonService.IsMasterClient)
                return;

            Debug.Log("Finishing round");
            RoundFinished?.Invoke(this, _lastRoundWonByTeamID);
        }

        public void FinishMatch()
        {
            Debug.Log("Finishing match");
            if (_isActive)
            {
                Stats.AddRound(new MatchStats.RoundStats
                {
                    WinningTeamID = TeamID.Neutral,
                    PlayTimeInSeconds =
                        (int) (_roundStartTime - GameManager.Instance.MatchTimer.GetCurrentTimerInSeconds())
                });
            }

            GetPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                if (players[i].GameObject != null)
                    Stats.SetPlayTime(players[i], players[i].GameObject.GetComponent<PlayTimeTracker>().PlayTime);

            _isActive = false;
            OnGameStatsChanged();
            ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;
            Finished?.Invoke(this);
            GameManager.Instance.TriggerMatchFinishedEvent(this);
        }

        /// <summary>
        /// Call this to trigger StatsChanged event and sync the stats to clients.
        /// </summary>
        protected void OnGameStatsChanged()
        {
            StatsChanged?.Invoke(Stats);
        }

        protected abstract void OnPlayerDied(PlayerHealth playerHealth, IPlayer damageDealer, byte colliderType);

        /// <summary>
        /// Callback if a Pillar was claimed by a Team.
        /// </summary>
        protected virtual void OnPillarOwningTeamChanged(Claimable claimable, TeamID oldTeamID, TeamID newTeamID,
            IPlayer[] newOwner)
        {
            if (!PhotonService.IsMasterClient)
                return;

            // only check this if we are still playing (gameMode.isActive == true)
            if (!IsActive)
                return;

            if (newTeamID == TeamID.Neutral)
            {
                Debug.LogError("Team neutral has claimed a Pillar -> this should not happen!");
                return;
            }

            CalculateNumberOfPillarsOwnedByTeams();
        }

        /// <summary>
        /// Calculate distribution of Pillars and write them to game stats.
        /// simple & slow implementation -> iterate over all Pillars and simply count Pillars per Team (including SpawnPillars, excluding SpectatorPillars)
        /// </summary>
        protected void CalculateNumberOfPillarsOwnedByTeams()
        {
            if (!PhotonService.IsMasterClient)
                return;

            // write pillar distribution to stats
            GameModeHelper.WritePillarDistributionToGameModeStats_MasterOnly(ScenePillars, Stats, CountGoalPillars);

            // trigger GameStatsChanged event (to update ui & sync)
            OnGameStatsChanged();
        }

#endregion

#region Handle Player

        /// <summary>
        /// Register callbacks, init & spawn new Player, add new Player to stats.
        /// </summary>
        /// <param name="player">New Player to register.</param>
        private void OnPlayerAdded([NotNull] IPlayer player)
        {
            if (!PhotonService.IsMasterClient) return;
            if (IsMatchFull && player.IsBot) player.PlayerNetworkEventHandler.SendDisconnectPlayer();

            // Home Version
            if (TowerTagSettings.Home)
            {
                // Count Bots in Match
                var botInMatch = PlayerManager.Instance.GetAllParticipatingAIPlayerCount() > 0;

                if (IsMatchFull && !botInMatch)
                {
                    player.PlayerNetworkEventHandler.SendDisconnectPlayer();
                    Debug.LogError("Match is Full -> Kick late joiner");
                }
            }

            if (player.PlayerHealth == null)
            {
                Debug.LogError("Cannot register Player because his PlayerHealth is null!");
                return;
            }

            // register DamageModel
            player.PlayerHealth.PlayerDied += OnPlayerDied;

            if (!_masterClientsSwitched)
            {
                player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.DeadButNoLimbo);
                if (MatchStarted)
                {
                    HandleLateJoiner(player);
                    GameManager.Instance.SyncToLateJoiner(player);
                }
                else
                {
                    int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(
                        PhotonService.ServerTimestamp,
                        GameManager.Instance.MatchStartCountdownTimeInSec + GameManager.Instance.CountdownDelay);
                    RespawnPlayer(player, (int) _roundStartTime, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Respawn);
                }
            }
            
            // add new Player to stats
            if (player.IsParticipating) Stats.AddPlayer(player);
        }

        /// <summary>
        /// Remove Player from the Match.
        /// </summary>
        /// <param name="player">Player who should leave the match.</param>
        private void OnPlayerRemoved(IPlayer player)
        {
            if (!PhotonService.IsMasterClient) return;

            var errorOccurred = false;

            if (player == null)
            {
                UnityEngine.Debug.LogError("Match:OnPlayerRemoved -> Player is null. This should not have happened.");
                errorOccurred = true;
            }

            if (!errorOccurred && player.PlayerHealth != null)
                player.PlayerHealth.PlayerDied -= OnPlayerDied;

            (bool finished, TeamID winningTeamID) = GetRoundStatus();
            if (IsActive && finished && BotManagerHome.Instance.ReplacingBot)
            {
                FinishRoundOnMaster(winningTeamID);
            }

            if (!errorOccurred)
                Stats.RemovePlayer(player);
        }

#region LateJoiner

        private void HandleLateJoiner(IPlayer player)
        {
            /*  Uncomment this block if you want to enable late join (Join a running match)
                for the Home version. Care, not stable atm (25.01.2021, PW) 
            if (TowerTagSettings.Home)
                HandleLateJoinerHome(player);
            else
            */
                HandleLateJoinerArcade(player);
        }

        private void HandleLateJoinerArcade(IPlayer player)
        {
            if (TowerTagSettings.Home && !player.IsBot) player.IsParticipating = false;
            player.PlayerHealth.KillPlayerFromMaster();
            int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonService.ServerTimestamp,
                GameManager.Instance.MatchStartCountdownTimeInSec + GameManager.Instance.CountdownDelay);
            switch (GameMode)
            {
                case GameMode.GoalTower:
                case GameMode.DeathMatch:
                    if (player.IsParticipating)
                    {
                        RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                            TeleportHelper.TeleportDurationType.Respawn);
                    }
                    else
                    {
                        TeleportHelper.TeleportPlayerToFreeSpectatorPillar(player,
                            TeleportHelper.TeleportDurationType.Immediate);
                    }

                    break;
                case GameMode.Elimination:
                    if (!player.IsParticipating ||
                        GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.PlayMatch)
                        TeleportHelper.TeleportPlayerToFreeSpectatorPillar(player,
                            TeleportHelper.TeleportDurationType.Immediate);
                    else if (player.IsParticipating)
                    {
                        RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                            TeleportHelper.TeleportDurationType.Respawn);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleLateJoinerHome(IPlayer player)
        {
            player.PlayerHealth.KillPlayerFromMaster();
            int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonService.ServerTimestamp,
                GameManager.Instance.MatchStartCountdownTimeInSec + GameManager.Instance.CountdownDelay);

            // Handle uneven Teams with no AI PLayer in Match
            var botInMatch = PlayerManager.Instance.GetAllParticipatingAIPlayerCount() > 0;

            if (!player.IsBot
                && PlayerManager.Instance.GetAllParticipatingAIPlayerCount() > 0)
            {
                // check if Match is full
                if (IsMatchFull && botInMatch)
                {
                    // Kick Bot and replace with player
                    ReplaceBotWithPlayer(player, respawnAt, GameMode);
                }
                else // Match is not full atM
                {
                    // check if Bot Players available to kick
                    if (botInMatch)
                    {
                        // Kick Bot and replace with player
                        ReplaceBotWithPlayer(player, respawnAt, GameMode);
                    }
                    else
                    {
                        if (IsMatchFull)
                        {
                            player.IsParticipating = !IsMatchFull;
                            HandleLateJoinerArcade(player);
                        }
                        else
                        {
                            // spawn player in team...if teams are even you should spawn a new bot player too
                            IntegrateLateJoinerInRunningMatch(player, respawnAt, GameMode);
                        }
                    }
                }
            }
            else
            {
                player.IsParticipating = !IsMatchFull;
                HandleLateJoinerArcade(player);
            }
        }

        private void IntegrateLateJoinerInRunningMatch(IPlayer player, int respawnAt, GameMode matchDescriptionGameMode)
        {
            var isPlayerInNeutralTeam = player.TeamID == TeamID.Neutral;
            var areTeamsEven = PlayerManager.Instance.GetParticipatingIcePlayerCount() 
                               == PlayerManager.Instance.GetParticipatingFirePlayerCount();
            var smallerTeam = PlayerManager.Instance.GetParticipatingIcePlayerCount() <=
                                            PlayerManager.Instance.GetParticipatingFirePlayerCount()
                ? TeamID.Ice
                : TeamID.Fire;

            if (isPlayerInNeutralTeam)
            {
                if (areTeamsEven)
                {
                    var team = (TeamID) new Random().Next(2);
                    player.SetTeam(team);
                    Stats.ResetStats(player);
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);

                    // Spawn Bot for even Teams
                    StaticCoroutine.StartStaticCoroutine(
                        BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(
                            player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire, 1));
                }
                else
                {
                    player.SetTeam(smallerTeam);
                    Stats.ResetStats(player);
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                }
            }
            else
            {
                if (areTeamsEven)
                {
                    Stats.ResetStats(player);
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                }
                else
                {
                    Stats.ResetStats(player);
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);

                    // Spawn Bot for even Teams
                    StaticCoroutine.StartStaticCoroutine(
                        BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(
                            player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire, 1));
                }
            }
        }

        private void ReplaceBotWithPlayer(IPlayer player, int respawnAt, GameMode matchDescriptionGameMode)
        {
            bool botFound = PlayerManager.Instance.GetAIPlayerFromTeamWithMoreAIPlayer(out IPlayer bot);
            if (!botFound || bot == null)
            {
                Debug.LogWarning("Tried to replace a bot with player but no bot found");
                return;
            }

            TeamID botTeam = bot.TeamID;
            bool botIsAlive = bot.IsAlive;
            BotManagerHome.Instance.ReplacingBot = true;
            BotManagerHome.Instance.DestroyBots(new[] {bot}, 1);
            //PhotonNetwork.Destroy(bot.GameObject);
            player.SetTeam(botTeam);
            Stats.ResetStats(player);
            switch (matchDescriptionGameMode)
            {
                case GameMode.GoalTower:
                case GameMode.DeathMatch:
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                    break;
                case GameMode.Elimination:
                    if (botIsAlive)
                    {
                        RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                            TeleportHelper.TeleportDurationType.Immediate);
                    }
                    //UnityEngine.Debug.LogError("Bot is replaced");
                    PrepareRespawn(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);
                    RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch,
                        TeleportHelper.TeleportDurationType.Immediate);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            player.IsParticipating = true;
        }

#endregion

        protected abstract (bool finished, TeamID winningTeamID) GetRoundStatus();

        private static void TeleportPlayerToSpectatorPillarAsSpectator(IPlayer player, int startTimeStamp,
            CountdownType countdownType, TeleportHelper.TeleportDurationType teleportDurationType)
        {
            //deactivate Player and teleport to spawn pillar
            TeleportHelper.TeleportPlayerToFreeSpectatorPillar(player, teleportDurationType);
            player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Dead);

            //Delayed Respawn player and show countdown while waiting
            if (player.GameObject != null)
            {
                var playerNetworkEventHandler = player.GameObject.GetComponent<PlayerNetworkEventHandler>();
                playerNetworkEventHandler.SendTimerActivation(startTimeStamp, countdownType);
            }
        }

        protected static void PrepareRespawn(IPlayer player, int startTimeStamp,
            CountdownType countdownType, TeleportHelper.TeleportDurationType teleportDurationType)
        {
            TeleportHelper.TeleportPlayerOnSpawnPillar(player, teleportDurationType);
            player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Dead);
            if (player.GameObject != null)
            {
                var playerNetworkEventHandler = player.GameObject.GetComponent<PlayerNetworkEventHandler>();
                playerNetworkEventHandler.SendTimerActivation(startTimeStamp, countdownType);
            }
        }

        private IEnumerator WaitForSpawnPillar(IPlayer player, int startTimeStamp,
            CountdownType countdownType, TeleportHelper.TeleportDurationType teleportDurationType)
        {
            float timer = 0;
            while (PillarManager.Instance.FindSpawnPillarForPlayer(player) == null
                    && timer < 2)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            PrepareRespawn(player,startTimeStamp,countdownType,teleportDurationType);
        }

        private static void RespawnPlayer(IPlayer player)
        {
            if (player.TeleportHandler.CurrentPillar.IsSpectatorPillar) return;
            player.ResetPlayerHealthOnMaster();
            bool gunDisabled = TTSceneManager.Instance.IsInCommendationsScene;
            const bool isImmortal = false;
            const bool isInLimbo = false;
            player.PlayerStateHandler.SetPlayerStateOnMaster(new PlayerState(isImmortal, gunDisabled, isInLimbo));
        }

        protected void RespawnPlayer(IPlayer player, int timestamp, CountdownType countdownType,
            TeleportHelper.TeleportDurationType teleportType)
        {
            ((Player) player).StartCoroutine(WaitForSpawnPillar(player, timestamp, countdownType, teleportType));
            ((Player) player).StartCoroutine(RespawnPlayerCoroutine(player, timestamp));
        }

        private IEnumerator RespawnPlayerCoroutine(IPlayer player, int timeStamp)
        {
            float startTime = GameManager.Instance.MatchTimer.GetRemainingMatchTimeInSeconds();
            float waitPeriod = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                PhotonService.ServerTimestamp,
                timeStamp);
            while (GameManager.Instance.MatchTimer.GetRemainingMatchTimeInSeconds() > startTime - waitPeriod)
            {
                if (!_isActive)
                    yield break; // cancel respawn on round finish. Respawn is then triggered by StartNewRound
                yield return null;
            }

            player.IsLateJoiner = false;
            player.ResetPlayerHealthOnMaster();
            player.PlayerStateHandler.SetPlayerStateOnMaster(TTSceneManager.Instance.IsInCommendationsScene
                ? PlayerState.AliveButDisabled
                : PlayerState.Alive);
        }

        public void OnMasterClientSwitched(ConnectionManager sender, Photon.Realtime.Player newMasterClient)
        {
            if (newMasterClient.IsLocal)
            {
                _masterClientsSwitched = true;
                InitMatchOnMaster();

                PlayerManager.Instance.GetOwnPlayer()?.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Alive);
            }
            else
            {
                GameManager.Instance.GameManagerAddedPlayer -= OnPlayerAdded;
                PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            }
        }

        /// <summary>
        /// Returns array of registered players
        /// </summary>
        /// <returns></returns>
        public void GetRegisteredPlayers(out IPlayer[] players, out int count) => GetPlayers(out players, out count);
        public int GetRegisteredPlayerCount()
        {
            return GetPlayersCount();
        }

#endregion

#region Serialization

        /// <summary>l
        /// Serialize the internal state:
        /// - call it with writeStream to write the internal state to stream
        /// - call it with readStream to deserialize the internal state from stream
        /// </summary>
        /// <param name="stream">Stream to read from or write your data to.</param>
        /// <returns>True if succeeded read/write, false otherwise.</returns>
        public virtual bool Serialize(BitSerializer stream)
        {
            bool success = stream.SerializeUncompressed(ref _matchStartAtTimestamp);
            success = success && stream.SerializeUncompressed(ref _matchFinishedAtTimestamp);
            success = success && stream.SerializeUncompressed(ref _roundStartAtTimestamp);
            success = success && stream.SerializeUncompressed(ref _roundFinishedAtTimestamp);
            success = success && stream.Serialize(ref _lastRoundWonByTeamID);
            success = success && stream.Serialize(ref _matchTimeInSeconds,
                BitCompressionConstants.MinMatchTime,
                BitCompressionConstants.MaxMatchTime);
            success = success && stream.Serialize(ref _isActive);
            success = success && Stats.Serialize(stream);

            if (stream.IsReading)
            {
                OnGameStatsChanged();
            }

            return success;
        }

#endregion

#region Cleanup

        /// <summary>
        /// Is called when Garbage collector kicks in, so cleanup!
        /// </summary>
        public void Dispose()
        {
            Cleanup();
        }

        /// <summary>
        /// Cleanup Match: sign off player, clear events, ...
        /// </summary>
        private void Cleanup()
        {
            GameManager.Instance.GameManagerAddedPlayer -= OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;

            if (ScenePillars != null)
            {
                foreach (Pillar p in ScenePillars)
                {
                    p.OwningTeamChanged -= OnPillarOwningTeamChanged;
                }

                ScenePillars = null;
            }

            GameModeHelper.Cleanup();
        }

#endregion

#region Debug

        public string PrintMatch()
        {
            var s = new StringBuilder("AbstractMatchBase: \n");
            s.Append("MatchID: ").AppendLine(MatchID.ToString());
            s.Append("Scene to Load: ").AppendLine(Scene);
            return s.ToString();
        }

#endregion
    }
}