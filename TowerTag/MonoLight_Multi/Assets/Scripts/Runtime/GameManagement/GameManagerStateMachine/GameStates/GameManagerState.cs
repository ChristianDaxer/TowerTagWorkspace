using TowerTag;

public partial class GameManager {
    /// <summary>
    /// Base state for GameManagerStateMachine
    /// </summary>
    public abstract class
        GameManagerState : SerializableState<GameManagerStateMachine, GameManager, GameManagerState> {
        private int _enteredTimestamp;
        protected IPhotonService PhotonService { get; private set; }

        protected int EnteredTimestamp => _enteredTimestamp;
        protected GameManager GameManagerInstance => StateMachineContext;

        /// <summary>
        /// Identifies the current state.
        /// </summary>
        public abstract GameManagerStateMachine.State StateIdentifier { get; }

        protected override void Init() {
            PhotonService = ServiceProvider.Get<IPhotonService>();
        }

        /// <summary>
        /// EnterState is called when the state is set as current state (by ChangeState() function).
        /// Use this to initialize a state.
        /// </summary>
        public override void EnterState() {
            if (PhotonService.IsMasterClient) {
                _enteredTimestamp = PhotonService.ServerTimestamp;
            }
        }

        /// <summary>
        /// UpdateState is called while we are set as current state.
        /// Use this to update a state.
        /// </summary>
        public override void UpdateState() { }

        /// <summary>
        /// ExitState is called when another state is set by ChangeState().
        /// Use this to cleanup the state.
        /// </summary>
        public override void ExitState() { }

        /// <summary>
        /// Implement this if you want to react on a Pause message (called on Master client only).
        /// </summary>
        /// <param name="pause">True if Pause should be triggered, false if we should resume from pause. </param>
        public virtual void SetPauseMatch(bool pause) {
            Debug.Log($"GameManagerState.SetPauseMatch: {pause} ({StateIdentifier})");
        }

        /// <summary>
        /// Transition to the match configuration hub.
        /// </summary>
        public virtual void LoadHub() {
            if (PhotonService.IsMasterClient) {
                StateMachine.ChangeState(GameManagerStateMachine.State.Configure);
            }
            else
                Debug.LogWarning("Tried to trigger hub-scene load on non-master client");
        }

        /// <summary>
        /// Transition to the match configuration hub.
        /// </summary>
        public void LoadOffboarding() {
            if (PhotonService.IsMasterClient) {
                StateMachine.ChangeState(GameManagerStateMachine.State.Offboarding);
            }
            else
                Debug.LogWarning("Tried to trigger hub-scene load on non-master client");
        }

        /// <summary>
        /// Implement this if you want to react on a PlayerSync message (called on Master client only).
        /// Use this to handle PlayerSyncBarriers (if needed), so we wait for all Players to send us the right message before we go on.
        /// </summary>
        /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
        /// <param name="player">Player who send the message.</param>
        public virtual void OnReceivedPlayerSyncInfo(int matchID, IPlayer player) {
            string playerID = player == null ? "null" : player.PlayerID.ToString();
            Debug.LogWarning($"Late Joiner: Player: {playerID} (game state {StateIdentifier})");
        }

        /// <summary>
        /// Please implement synchronisation of internal state in every subclass (if needed).
        /// 
        /// Serialize:
        /// - call it with writeStream to write the internal state to stream
        /// - call it with readStream to deserialize the internal state from stream
        /// </summary>
        /// <param name="stream">Stream to read from or write your data to.</param>
        /// <returns>True if succeeded read/write, false otherwise.</returns>
        public override bool Serialize(BitSerializer stream) {
            stream.SerializeUncompressed(ref _enteredTimestamp);
            return true;
        }

        /// <summary>
        /// Print State internals to string (and to console if printToLog is true).
        /// </summary>
        /// <param name="printToLog">Should the returned string also printed to console/logFile?</param>
        /// <returns>String with internal members to view in DebugUI.</returns>
        public virtual string PrintState(bool printToLog) {
            string s = $"State: {StateIdentifier}";

            if (printToLog)
                Debug.Log(s);

            return s;
        }
    }
}