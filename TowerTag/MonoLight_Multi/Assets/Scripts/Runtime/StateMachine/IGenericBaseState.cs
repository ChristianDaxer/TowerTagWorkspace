public interface IGenericBaseState<T>
{
    void InitState(T stateMachine);
    void EnterState();
    void UpdateState();
    void ExitState();
}
