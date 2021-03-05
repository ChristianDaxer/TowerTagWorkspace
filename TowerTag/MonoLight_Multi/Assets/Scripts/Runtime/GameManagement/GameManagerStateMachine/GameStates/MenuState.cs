using static GameManager.GameManagerStateMachine;

partial class GameManager {
    private class MenuState : GameManagerState {
        public override State StateIdentifier => State.Undefined;

        //Todo: Controlled scene change from here in enter state
        public override void EnterState() {
            base.EnterState();
            GameManagerInstance.CurrentHomeMatchType = HomeMatchType.Undefined;
            if(GameManagerInstance.CurrentMatch != null) GameManagerInstance.CurrentMatch.IsLoaded = false;
        }
    }
}