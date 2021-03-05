using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Basic.UnityParticleSystem;
using Network;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerTag {
    public static class HubSceneBehaviour {
        /// <summary>
        /// List of registered player for this match.
        /// </summary>
        private static readonly List<IPlayer> _players = new List<IPlayer>();

        private static bool IsOnlyOnePlayerParticipating => PlayerManager.Instance.GetParticipatingPlayersCount() < 2;

        /// <summary>
        /// Returns if a added player should get forced to the team with lower amount of player
        /// </summary>
        private static bool ShouldTeamsGetLeveledOut => TowerTagSettings.BasicMode || TowerTagSettings.Home
            && !GameManager.Instance.TrainingVsAI
            && TTSceneManager.Instance.IsInHubScene;

        public static void SetUpHub() {
            PlayerManager.Instance.PlayerAdded += AddPlayer;
            PlayerManager.Instance.PlayerRemoved += RemovePlayer;

            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                AddPlayer(players[i]);

            ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene newScene, LoadSceneMode arg1) {
            if (!PhotonNetwork.IsMasterClient) return;
            if (TTSceneManager.Instance == null) return;
            if (TTSceneManager.Instance.PreviousScene == TTSceneManager.Instance.CurrentHubScene)
                Cleanup();
        }

        private static void AddPlayer(IPlayer player)
        {
            if (player == null) {
                Debug.LogErrorFormat("Cannot add NULL player.");
                return;
            }

            if (!PhotonNetwork.IsMasterClient) return;
            if (!_players.Contains(player)) {
                _players.Add(player);
                player.PlayerHealth.PlayerDied += RespawnOnCurrentPillar;
            }


            // reassign team, if full
            if (TeamManager.Singleton.Get(player.TeamID).PlayerCountWithoutAI() > TowerTagSettings.MaxTeamSize) {
                player.SetTeam(player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire);
            }

            if (player.TeamID != TeamID.Neutral) {
                // level out teams in basic mode
                if (ShouldTeamsGetLeveledOut) {
                    if (!player.IsBot) {
                        TeamID otherTeamID = player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire;
                        if (TeamManager.Singleton.Get(otherTeamID).PlayerCountWithoutAI() <
                            TeamManager.Singleton.Get(player.TeamID).PlayerCountWithoutAI() - 1 &&
                            player.DefaultName != BotSpawner.DefaultDebugBotName) {
                            player.SetTeam(otherTeamID);
                        }
                    }
                }
            }
            else {
                TeamID smallerTeamID = TeamManager.Singleton.GetIdOfSmallerTeam(out bool isTeamFull);
                if (!isTeamFull)
                    player.SetTeam(smallerTeamID);
            }

            if (RoomOptionManager.HasRoomTooManyPlayers
                && PlayerManager.Instance.IsAtLeastOneBotParticipating)
                ReplaceBotWithNewPlayer(player);
            
            if (player.PlayerState.IsDead)
                RespawnOnCurrentPillar(player.PlayerHealth, null, 0);

            if (IsOnlyOnePlayerParticipating)
                SpawnBotInEnemyTeam();
        }

        private static void RemovePlayer(IPlayer player) {
            if (!PhotonNetwork.IsMasterClient) return;
            if (_players.Contains(player)) {
                _players.Remove(player);
                player.PlayerHealth.PlayerDied -= RespawnOnCurrentPillar;
            }
        }

        private static void ReplaceBotWithNewPlayer(IPlayer player)
        {
            PlayerManager.Instance.GetAllParticipatingAIPlayers(out var players, out var count);
            IPlayer bot = players[0];
            TeamID botTeam = bot.TeamID;
            BotManagerHome.Instance.ReplacingBot = true;
            BotManagerHome.Instance.DestroyBots(new[] {bot}, 1);
            player.SetTeam(botTeam);
        }

        private static void SpawnBotInEnemyTeam()
        {
            var ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            var botTeamID = TeamID.Neutral;
            if (ownPlayer != null)
            {
                botTeamID = ownPlayer.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire;
            }

            StaticCoroutine.StartStaticCoroutine(
                BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(
                    botTeamID, 1));
        }

        public static void RespawnOnCurrentPillar(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage,
            byte colliderType) {
            if (!PhotonNetwork.IsMasterClient)
                return;

            int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonNetwork.ServerTimestamp,
                BalancingConfiguration.Singleton.RoundStartCountdownTimeInSec);
            respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(respawnAt,
                GameManager.Instance.CountdownDelay);

            RespawnPlayer(playerHealth.Player, respawnAt);
        }


        private static void RespawnPlayer(IPlayer player, int timestamp) {
            player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Dead);
            player.PlayerNetworkEventHandler.SendTimerActivation(timestamp, Match.CountdownType.StartRound);
            ((Player) player).StartCoroutine(RespawnPlayerCoroutine(player, timestamp));
        }

        private static IEnumerator RespawnPlayerCoroutine(IPlayer player, int timeStamp) {
            float waitPeriod = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                PhotonNetwork.ServerTimestamp,
                timeStamp);

            yield return new WaitForSeconds(waitPeriod);

            if (!TTSceneManager.Instance.IsInHubScene && !TTSceneManager.Instance.IsInTutorialScene)
                yield break;

            player.ResetPlayerHealthOnMaster();
            player.PlayerStateHandler.SetPlayerStateOnMaster(
                GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Commendations
                || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing
                    ? PlayerState.AliveButDisabled
                    : PlayerState.Alive);
        }

        private static void OnMasterClientSwitched(ConnectionManager arg1, Photon.Realtime.Player arg2)
        {
            if(arg2.IsLocal)
            {
                foreach (var player in _players)
                {
                    if (player.PlayerState.IsDead)
                        RespawnOnCurrentPillar(player.PlayerHealth, null, 0);
                }
            }
        }

        private static void Cleanup() {
            PlayerManager.Instance.PlayerAdded -= AddPlayer;
            PlayerManager.Instance.PlayerRemoved -= RemovePlayer;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            for (int i = _players.Count - 1; i >= 0; i--) {
                RemovePlayer(_players[i]);
            }
        }
    }
}