public partial class GameManager {
    private class MissionBriefingState : GameManagerMatchState {
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.MissionBriefing;
        private const int MissionBriefingTimeInSeconds = 12;
        private int _matchID;

        public override void EnterState() {
            base.EnterState();
            MatchDescription matchDescription;
            if (PhotonService.IsMasterClient) {
                matchDescription = GameManagerInstance.MatchDescription;
                _matchID = matchDescription.MatchID;
            }
            else {
                matchDescription = MatchDescriptionCollection.Singleton._matchDescriptions[_matchID];
                GameManagerInstance.MatchDescription = matchDescription;
            }

            if (matchDescription != null) {
                AnalyticsController.ShowMissionBriefing(
                    matchDescription.MapName,
                    ConfigurationManager.Configuration.Room);
            }

            GameManagerInstance.ActivateAllPlayersOnMaster(false, true);

            if (!TTSceneManager.Instance.IsInHubScene) GameManagerInstance.ConfigureMatch();
            GameManagerInstance.StartMissionBriefing(GameManagerInstance.CurrentMatch.GameMode);
        }

        public override void UpdateState() {
            if (!PhotonService.IsMasterClient) {
                return;
            }

            if (HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(EnteredTimestamp,
                    PhotonService.ServerTimestamp) >= MissionBriefingTimeInSeconds) {
                StateMachine.ChangeState(GameManagerStateMachine.State.LoadMatch);
            }
        }

        public override bool Serialize(BitSerializer stream) {
            bool success = base.Serialize(stream);
            return success && stream.SerializeUncompressed(ref _matchID);
        }
    }
}