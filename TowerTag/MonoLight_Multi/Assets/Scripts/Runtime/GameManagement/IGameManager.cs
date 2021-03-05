namespace GameManagement {
    public interface IGameManager {
        MatchTimer MatchTimer { get; }
        IMatch CurrentMatch { get; }
        GameManager.GameManagerStateMachine.State CurrentState { get; }

        void Tick();
        void OnApplicationQuit();
    }
}