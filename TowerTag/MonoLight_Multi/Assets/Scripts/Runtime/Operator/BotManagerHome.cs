using System;
using System.Collections;
using AI;
using Photon.Pun;
using Photon.Realtime;
using TowerTag;
using UnityEngine;
using VRNerdsUtilities;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using IPlayer = TowerTag.IPlayer;
using Player = Photon.Realtime.Player;
using Random = UnityEngine.Random;

public class BotManagerHome : SingletonMonoBehaviour<BotManagerHome>, IInRoomCallbacks {
    public bool ReplacingBot;

    private void OnEnable() {
        RegisterEventListener();
    }

    private void OnDisable() {
        UnregisterEventListener();
    }

    private void RegisterEventListener() {
        GameManager.Instance.MatchConfigurationStarted += OnHubSceneLoaded;            
        //GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
    }

    private void UnregisterEventListener() {
        GameManager.Instance.MatchConfigurationStarted -= OnHubSceneLoaded;
        //GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    private void OnBasicCountdownStarted(float countdownTime) {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!GameManager.Instance.TrainingVsAI &&
            PlayerManager.Instance.GetUnbalancedTeam(out (int ice, int fire) difference)) {
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("C1", out var maxPlayers);

            if (maxPlayers == null) {
                Debug.LogWarning("Error. Cant find max Players Value in custom Properties");
            }

            if ((byte) maxPlayers == 2) {
                Debug.Log("Map allows a maximum of 1 player per team! Ignoring uneven (min. 2vs2) rule!");
                if (PlayerManager.Instance.GetParticipatingPlayersCount() < 2) {
                    var emptyTeam = PlayerManager.Instance.GetParticipatingPlayersCount() > 0
                        ? TeamID.Fire
                        : TeamID.Ice;
                    StartCoroutine(SpawnBotForTeamWhenOwnPlayerAvailable(emptyTeam, 1));
                }
            }
            else {
                if (difference.fire > 0) StartCoroutine(SpawnBotForTeamWhenOwnPlayerAvailable(TeamID.Fire, difference.fire));
                if (difference.ice > 0) StartCoroutine(SpawnBotForTeamWhenOwnPlayerAvailable(TeamID.Ice, difference.ice));
            }
        }
    }

    private void OnHubSceneLoaded() {
        if (!PhotonNetwork.IsMasterClient) return;
        PlayerManager.Instance.GetAllAIPlayers(out var players, out var count);
        if (count > 0) {
            StartCoroutine(DestroyBots(players, count));
        }
    }

    public IEnumerator DestroyBots(IPlayer[] players, int count) {
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            PhotonNetwork.Destroy(players[i].GameObject);
        }
    }

    public void DestroyBot(IPlayer[] players, int count) {
        StartCoroutine(DestroyBots(players, count));
    }

    /// <summary>
    /// Home: This Coroutine waits until the local player is initialized before adding bots
    /// </summary>
    /// <param name="team"></param>
    /// <param name="botCount">The amount of bots you want to add to the team</param>
    /// <param name="botSpawnCondition"></param>
    /// <param name="botLevel">The difficulty the bot should have</param>
    /// <param name="spawnForOtherTeam"></param>
    /// <param name="maxPlayersFill"></param>
    /// <returns></returns>
    public IEnumerator SpawnBotForTeamWhenOwnPlayerAvailable(TeamID team, int botCount,
        Func<bool> botSpawnCondition = null,
        BotBrain.BotDifficulty botLevel = BotBrain.BotDifficulty.Medium, bool spawnForOtherTeam = false, int maxPlayersFill = -1) {
        SpawningBots = true;
        if (botCount == 0) yield break;

        //This is for later, when random joiners cant join custom rooms. We want want Bots in Custom Rooms
        if (TowerTagSettings.Home &&
            GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing)
            yield break;

        float timer = 0;
        float maxWaitTime = 5;
    
        while (PlayerManager.Instance.GetOwnPlayer() == null && timer <= maxWaitTime) {
            yield return new WaitForSeconds(0.5f);
        }

        if (botSpawnCondition != null)
            yield return new WaitUntil(botSpawnCondition);

        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();

        if (ownPlayer != null) {
            // Debug.Log("Bot count is: " + botCount + "! So were filling " + (maxPlayersFill != -1 ? Mathf.Clamp(botCount, 0, maxPlayersFill / 2) : botCount) + "bots for team " + team.ToString());

            for (int i = 0; i < (maxPlayersFill != -1 ? Mathf.Clamp(botCount, 0, maxPlayersFill / 2) : botCount); i++) {
                TeamID spawnTeam = spawnForOtherTeam
                    ? (TeamManager.Singleton.GetEnemyTeamIDOfPlayer(ownPlayer))
                    : team;

                IPlayer bot = BotManager.Instance.AddBot(spawnTeam, botLevel);
                yield return new WaitUntil(() => bot.GameObject.CheckForNull());
            }
        }
        else {
            Debug.LogError("Can't init bot, because no local player was found!");
        }

        SpawningBots = false;
    }

    public bool SpawningBots { get; private set; }

    private void OnPlayerRemoved(IPlayer player) {
        if (!PhotonNetwork.IsMasterClient || !player.IsParticipating || ReplacingBot) {
            ReplacingBot = false;
            return;
        }

        float time = Time.time;
        float botSpawnDelay = 0.5f;
        switch (GameManager.Instance.CurrentState) {
            case GameManager.GameManagerStateMachine.State.MissionBriefing:
            case GameManager.GameManagerStateMachine.State.LoadMatch:
            case GameManager.GameManagerStateMachine.State.PlayMatch:
            case GameManager.GameManagerStateMachine.State.Countdown:
            case GameManager.GameManagerStateMachine.State.RoundFinished:
            case GameManager.GameManagerStateMachine.State.MatchFinished:
            case GameManager.GameManagerStateMachine.State.Paused:
            case GameManager.GameManagerStateMachine.State.Emergency:
            {
                StartCoroutine(SpawnBotForTeamWhenOwnPlayerAvailable(player.TeamID, 1, () => Time.time - time >= botSpawnDelay));
                break;
            }
        }

        ReplacingBot = false;
    }

    #region PhotonCallBacks

    public void OnPlayerEnteredRoom(Player newPlayer) {
    }

    public void OnPlayerLeftRoom(Player player) {
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
    }

    public void OnMasterClientSwitched(Player newMasterClient) {
    }

    #endregion
}