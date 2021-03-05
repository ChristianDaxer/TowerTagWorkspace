using System.Linq;
using JetBrains.Annotations;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyTowerUI {
    public sealed class VotingObserverBasic : VotingObserver {
        [SerializeField] private ReadyTowerUiGameModeSelectionController _modeSelection;
        [SerializeField] private Text _voteForStartStanding;
        private IPlayer _player;
        private ReadyTowerUiGameModeButton[] Modes => _modeSelection.GameModeButtons;

        private void Awake() {
            _player = PlayerManager.Instance.GetOwnPlayer();
        }

        protected override void OnEnable() {
            base.OnEnable();
            _modeSelection.NewVoteEntered += OnNewVoteEntered;
            ReadyTowerUiController.VoteForStartButtonPressed += OnVoteForStartPressed;
            ReadyTowerUiController.RequestTeamChangeButtonPressed += OnTeamChangeRequestButtonPressed;
            UpdateStartNowVoting();
        }

        protected override void OnDisable() {
            base.OnDisable();
            _modeSelection.NewVoteEntered -= OnNewVoteEntered;
            ReadyTowerUiController.VoteForStartButtonPressed -= OnVoteForStartPressed;
            ReadyTowerUiController.RequestTeamChangeButtonPressed -= OnTeamChangeRequestButtonPressed;
        }

        protected override void OnPlayerAdded(IPlayer player) {
            base.OnPlayerAdded(player);
            UpdateStartNowVoting();
        }

        protected override void OnPlayerRemoved(IPlayer player) {
            base.OnPlayerRemoved(player);
            UpdateStartNowVoting();
            //4 = minimum player count for goal tower
            ReadyTowerUiGameModeButton gtButton = Modes.FirstOrDefault(mode => mode.Mode == GameMode.GoalTower);
            if (gtButton != null && !gtButton.IsGameModePlayable()) {
                gtButton.RemoveAllVoteIcons();
                if (_player.VoteGameMode != GameMode.GoalTower) return;
                _player.VoteGameMode = GameMode.UserVote;
                _modeSelection.TogglePlayerVoteStatus(GameMode.GoalTower, false);
            }
        }

        private void OnNewVoteEntered() {
            if (!PhotonNetwork.IsMasterClient) return;
            UpdateVotedModes();
            int voteCount = GetVoteCount();
            if (ReadyTowerPlayerLineManager.SlotCount == voteCount
                && PlayerManager.Instance.GetParticipatingFirePlayerCount() > 0
                && PlayerManager.Instance.GetParticipatingIcePlayerCount() > 0)
                GameManager.Instance.StartBasicMatch();
        }

        private int GetVoteCount() {
            var votes = 0;
            Modes.ForEach(mode => votes += mode.VoteCount);
            return votes;
        }

        private void UpdateVotedModes() {
            var highestVoteValue = 0;
            foreach (ReadyTowerUiGameModeButton modeButton in Modes) {
                if (modeButton.VoteCount < highestVoteValue) continue;

                if (modeButton.VoteCount == highestVoteValue) {
                    VotedGameModes.Add(modeButton.Mode);
                    continue;
                }

                if (modeButton.VoteCount > highestVoteValue) {
                    VotedGameModes.Clear();
                    VotedGameModes.Add(modeButton.Mode);
                    highestVoteValue = modeButton.VoteCount;
                }
            }
        }

        protected override void OnStartNowVoteChanged(IPlayer player, bool newState) {
            UpdateStartNowVoting();
            base.OnStartNowVoteChanged(player, newState);
        }

        private void UpdateStartNowVoting() {
            PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var count);
            _voteForStartStanding.text =
                $"({players.Take(count).Count(player => player.StartVotum)} / {count})";
        }

        [UsedImplicitly]
        public void OnVoteForStartPressed(ReadyTowerUiController sender, bool newValue) {
            if (_player != null) _player.StartVotum = newValue;
        }

        [UsedImplicitly]
        public void OnTeamChangeRequestButtonPressed(ReadyTowerUiController sender, bool newValue) {
            if (_player != null) _player.TeamChangeRequested = newValue;
        }
    }
}