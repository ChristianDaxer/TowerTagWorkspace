public partial class GameManager {
    /// <summary>
    /// State to load the Hub scene and configure a next Match.
    /// </summary>
    private class ConfigureMatchState : GameManagerState {

        private float _additionAfterPlayerLeft;


        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.Configure).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Configure;

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            GameManagerInstance.ConfigureMatch();
            if (TowerTagSettings.Home) QueueTimerManager.ResetAndStartQueueTimer();
        }

        public override void UpdateState() {
            if (TowerTagSettings.Home) QueueTimerManager.Tick();
        }

        public override void ExitState() {
            base.ExitState();
            QueueTimerManager.StopQueueTimer();
        }

        #endregion
    }
}