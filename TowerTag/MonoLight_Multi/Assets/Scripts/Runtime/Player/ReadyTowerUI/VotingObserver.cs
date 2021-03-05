using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine;

namespace ReadyTowerUI {
    public class VotingObserver : MonoBehaviour {
        private readonly Dictionary<IPlayer, GameMode> _votedModes = new Dictionary<IPlayer, GameMode>();
        public static List<GameMode> VotedGameModes { get; } = new List<GameMode>();

        protected virtual void OnEnable() {
            PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                players[i].StartNowVoteChanged += OnStartNowVoteChanged;
                players[i].GameModeVoted += OnGameModeVoted;
            }
        }

        protected virtual void OnDisable() {
            PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                players[i].StartNowVoteChanged -= OnStartNowVoteChanged;
                players[i].GameModeVoted -= OnGameModeVoted;
            }
        }

        protected virtual void OnPlayerAdded(IPlayer player) {
            player.StartNowVoteChanged += OnStartNowVoteChanged;
            player.GameModeVoted += OnGameModeVoted;
        }

        protected virtual void OnPlayerRemoved(IPlayer player) {
            player.StartNowVoteChanged -= OnStartNowVoteChanged;
            player.GameModeVoted -= OnGameModeVoted;
        }

        private void OnGameModeVoted(IPlayer player, (GameMode mode, GameMode previous) gameModeData) {
            if (gameModeData.mode != GameMode.UserVote) {
                if (!_votedModes.ContainsKey(player))
                    _votedModes.Add(player, gameModeData.mode);
                else {
                    _votedModes.Remove(player);
                    _votedModes.Add(player, gameModeData.mode);
                }
            }
            else {
                if (_votedModes.ContainsKey(player))
                    _votedModes.Remove(player);
            }

            UpdateVotedModes();
        }

        private void UpdateVotedModes() {
            Dictionary<GameMode, int> modeVotes = new Dictionary<GameMode, int>();
            _votedModes.ForEach(playerModePair => {
                if (modeVotes.ContainsKey(playerModePair.Value))
                    modeVotes[playerModePair.Value]++;
                else {
                    modeVotes.Add(playerModePair.Value, 1);
                }
            });

            if (modeVotes.Count <= 0) {
                VotedGameModes.Clear();
                return;
            }

            int highestVoteValue = 0;
            foreach (GameMode mode in modeVotes.Keys) {
                if (modeVotes[mode] < highestVoteValue) continue;

                if (modeVotes[mode] == highestVoteValue) {
                    VotedGameModes.Add(mode);
                    continue;
                }

                if (modeVotes[mode] > highestVoteValue) {
                    VotedGameModes.Clear();
                    VotedGameModes.Add(mode);
                    highestVoteValue = modeVotes[mode];
                }
            }
        }

        protected virtual void OnStartNowVoteChanged(IPlayer player, bool newState) {
            if (!PhotonNetwork.IsMasterClient) return;
            PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var count);
            if (count + PlayerManager.Instance.GetAIPlayerCount() >= 1 &&
                players.Take(count).Count(currentPlayer => currentPlayer.StartVotum) == count) {
                StartCoroutine(StartMatch());
            }
        }

        private IEnumerator StartMatch() {
            //PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("BF", out var fill);
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("C1", out var maxPlayers);

            bool unbalanced = PlayerManager.Instance.GetUnbalancedTeam(out (int ice, int fire) difference);

            if (!GameManager.Instance.TrainingVsAI &&
                /*(bool) fill &&*/ unbalanced) {
                print(PlayerManager.Instance.GetParticipatingIcePlayerCount());

                if (maxPlayers != null && ((byte) maxPlayers == 2 && difference.ice == 1 && difference.fire == 1)) {
                    Debug.Log("Map allows a maximum of 1 player per team! Ignoring uneven (min. 2vs2) rule!");
                }
                else {
                    if (maxPlayers != null && (difference.ice > 0 && PlayerManager.Instance.GetParticipatingIcePlayerCount() < (byte) maxPlayers / 2)) {
                        print("diff" + difference.ice);
                        yield return BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(TeamID.Ice, difference.ice,
                            maxPlayersFill: (byte) maxPlayers);
                    }

                    PlayerManager.Instance.GetUnbalancedTeam(out difference);

                    // print(PlayerManager.Instance.GetParticipatingFirePlayers().Length);

                    if (maxPlayers != null && (difference.fire > 0 && PlayerManager.Instance.GetParticipatingFirePlayerCount() < (byte) maxPlayers / 2)) {
                        yield return BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(TeamID.Fire, difference.fire,
                            maxPlayersFill: (byte) maxPlayers);
                    }

                    yield return new WaitUntil(() => {
                        bool balanced = !PlayerManager.Instance.GetUnbalancedTeam(out (int ice, int fire) diff);

                        // dirty fast fix, im so sorry
                        if (maxPlayers != null && ((byte) maxPlayers == 2 && diff.ice == 1 && diff.fire == 1)) {
                            Debug.Log("Map allows a maximum of 1 player per team! Ignoring uneven (min. 2vs2) rule!");
                            return true;
                        }

                        return balanced;
                    });
                    /*yield return new WaitUntil(() => !PlayerManager.Instance.GetUnbalancedTeam(out _));*/
                }
            }

            GameManager.Instance.StartMatch();

            // *****************
            // Need Auto Fill Toggle
            // *****************
            /*if (!PlayerManager.Instance.OneTeamIsZero())
            {
                GameManager.Instance.StartMatch();
            }
            else
            {
                MessageQueue.Singleton.AddVolatileMessage("Could not start match! No empty teams allowed!");
            }*/
        }
    }
}