public partial class GameManager {
    public abstract class GameManagerMatchState : GameManagerState {
        private IMatch CurrentMatch { get; set; }

        public override void EnterState() {
            base.EnterState();
            //Late joiner
            if (GameManagerInstance.CurrentMatch == null) GameManagerInstance.SetMatch(CurrentMatch);
            if (!GameManagerInstance.CurrentMatch.IsLoaded && !(this is CommendationsState)
                                                           && !(this is MissionBriefingState)) GameManagerInstance.LoadCurrentMatch();
        }

        /// <summary>
        /// Implement synchronization of internal state here:
        /// - sync Match to load (send from Master client to all other clients)
        /// </summary>
        /// <param name="stream">Stream to read from or write your data to.</param>
        /// <returns>True if succeeded read/write, false otherwise.</returns>
        public override bool Serialize(BitSerializer stream) {
            base.Serialize(stream);
            if (stream.IsWriting) {
                // cache current Match from GameManager (just for convenience)
                CurrentMatch = GameManagerInstance.CurrentMatch;
                if (CurrentMatch == null)
                    return false;

                // send if current match is valid so we can abort the serialization without corrupting the stream (read != write)
                bool matchIsValid = GameManagerInstance.CurrentMatch != null;
                stream.WriteBool(matchIsValid);

                // abort the serialization if match is not valid
                if (!matchIsValid) {
                    Debug.LogWarning("Aborting serialization of load match state: invalid match.");
                    return false;
                }

                // send match ID
                stream.WriteInt(CurrentMatch.MatchID, BitCompressionConstants.MinMatchID, BitCompressionConstants.MaxMatchID);
                stream.WriteInt((int) CurrentMatch.GameMode, 0, 99);

                // send match
                CurrentMatch.Serialize(stream);
            }
            else {
                // did Master send the match?
                bool matchIsValid = stream.ReadBool();

                if (!matchIsValid) {
                    Debug.LogWarning("Aborting deserialization of load match state: invalid match.");
                    return false;
                }

                // read matchID
                int matchID = stream.ReadInt(BitCompressionConstants.MinMatchID, BitCompressionConstants.MaxMatchID);
                GameMode mode = (GameMode) stream.ReadInt(0, 99);

                CurrentMatch = GameManagerInstance.CurrentMatch;
                //To avoid overrides when getting multiple deserialize calls
                if (CurrentMatch == null || CurrentMatch.MatchID != matchID) {
                    CurrentMatch = MatchConfigurator.CreateMatch(matchID, mode, PhotonService);
                }

                if (CurrentMatch == null) {
                    Debug.LogError("Cannot deserialize load-match-state: match is null");
                    return false;
                }

                GameManagerInstance.SetMatch(CurrentMatch);
                CurrentMatch.Serialize(stream);

                // set the received match in GameManager so we can load it in EnterState function
            }

            return true;
        }
    }
}