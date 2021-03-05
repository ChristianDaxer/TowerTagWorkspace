using System.Linq;
using TowerTag;
using UnityEngine.SceneManagement;

public partial class GameManager {
    /// <summary>
    /// State machine to handle game states and syncs them so we can handle all game related stuff on local clients.
    /// GameStateChanges triggered by external calls should only triggered on Master client!!!
    /// </summary>
    public class
        GameManagerStateMachine : SerializableStateMachine<GameManagerStateMachine, GameManager, GameManagerState> {
        #region public Member

        /// <summary>
        /// State descriptions of this State machine.
        /// - returned by lastState / currentState properties
        /// - used for ChangeState(States stateYouWantToSwitchTo) to switch to the given state
        /// </summary>
        public enum State {
            /// StateMachine is not initialized yet or the requested state is null (ignored by ChangeState function)
            Undefined,

            /// HubSceneState (we are in the Hub scene and want to configure a new Match)
            Configure,

            /// State were players are briefed about the upcoming match
            MissionBriefing,

            /// State that loads and initializes a Match
            LoadMatch,

            /// State we are in when a Match is running
            PlayMatch,

            /// State we are in when a countdown is shown before Start of a Match or a new Round
            Countdown,

            /// State we are in when a round was finished and we show the round stats
            RoundFinished,

            /// State we are in when a match was finished and we show the match stats
            MatchFinished,

            /// State we are in when the Match was paused
            Paused,

            /// State we are in when we received an Emergency stop
            Emergency,

            /// State that corresponds to the commendations scene at the end of a match
            Commendations,

            ///State we just get into then manually triggered by the operator
            Offboarding,

            Tutorial
        }

        private GameManager GameManager => StateMachineContext;

        /// <summary>
        /// The current state the State machine is in.
        /// </summary>
        public State CurrentStateIdentifier => CurrentState?.StateIdentifier ?? State.Undefined;

        #endregion

        #region States (corresponding to States enum)

        private readonly GameManagerState[] _states = {
            new ConfigureMatchState(),
            new MissionBriefingState(),
            new LoadMatchState(),
            new CountdownState(),
            new PlayMatchState(),
            new PauseState(),
            new RoundFinishedState(),
            new MatchFinishedState(),
            new CommendationsState(),
            new OffboardingState(),
            new EmergencyState(),
            new MenuState(),
            new TutorialState()
        };

        #endregion

        #region StateMachine related functions

        /// <summary>
        /// Init the State machine: Register & init all states, enter the default state (Configure).
        /// </summary>
        /// <param name="stateMachineContext">The Context all states can access to communicate with the environment (get values or call functions).</param>
        public override void InitStateMachine(GameManager stateMachineContext) {
            // init base class
            base.InitStateMachine(stateMachineContext);

            // init states
            foreach (GameManagerState state in _states) {
                state.InitState(this);
            }

            // set states & stateIDs to in base class
            SetStates(_states);

            // clear queue for incoming ChangeState commands
            BlockedIncomingSerializationQueue.Clear();
            IsBlocked = false;

            // set Last state to Undefined to mark as new Start of StateMachine (for Reconnects)
            LastState = null;
        }

        public void ChangeStateToDefault() {
            ChangeState(_states[0]);
        }

        /// <summary>
        /// Change the state of the State machine.
        /// </summary>
        /// <param name="newState">The new state to switch to.</param>
        public void ChangeState(State newState) {
            ChangeState(_states.First(state => state.StateIdentifier == newState));
        }

        public bool IsInMatchState() {
            return _states.First(state => state.StateIdentifier == CurrentStateIdentifier) is
                GameManagerMatchState;
        }

        #endregion

        #region State related functions

        /// <summary>
        /// This call forwards the SyncInfo (received from a client) to the current state.
        /// </summary>
        /// <param name="player">The client who send the SyncInfo.</param>
        /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
        public void OnReceivedPlayerSyncInfo(int matchID, IPlayer player) {
            CurrentState?.OnReceivedPlayerSyncInfo(matchID, player);
        }

        /// <summary>
        /// This call forwards the Pause trigger to the current state (only processed when we are in Pause or PlayMatch state).
        /// </summary>
        /// <param name="pause">Set to true if you want to pause the current Match (ignored if we are not in PlayMatch state), false if you want to resume from a Pause (ignored if we are not in Pause state).</param>
        public void SetPauseMatch(bool pause) {
            CurrentState?.SetPauseMatch(pause);
        }

        /// <summary>
        /// Returns if we are in Pause state.
        /// </summary>
        /// <returns>True if we are in Pause state, false otherwise.</returns>
        public bool IsPaused() {
            return CurrentStateIdentifier == State.Paused;
        }

        public void ConfigureMatch() {
            CurrentState?.LoadHub();
        }

        public void LoadOffboarding() {
            CurrentState?.LoadOffboarding();
        }

        /// <summary>
        /// Print State internals to string (and to console if printToLog is true).
        /// </summary>
        /// <param name="printToLog">Should the returned string also printed to console/logFile?</param>
        /// <returns>String with internal members to view in DebugUI.</returns>
        public string PrintCurrentState(bool printToLog) {
            string s = "GameManagerStateMachine: isBlocked: " + IsBlocked;
            s += "\n -> Queued States: ";
            foreach (GameManagerState state in DebugBlockedIncomingSerializationQueue) {
                s += (state != null ? state.StateIdentifier.ToString() : "null") + " | ";
            }

            s += "\n--------------------------Match Info--------------------------------------------\n";

            s += "Loaded Scene: " + SceneManager.GetActiveScene().name + "\n";

            if (GameManager.CurrentMatch != null)
                s += GameManager.CurrentMatch.PrintMatch();

            s += "\n-------------------------State Info---------------------------------------------\n";
            if (CurrentState != null)
                s += CurrentState.PrintState(printToLog);
            else
                s += "State: " + State.Undefined;

            if (printToLog)
                Debug.Log(s);

            return s;
        }

        #endregion
    }
}