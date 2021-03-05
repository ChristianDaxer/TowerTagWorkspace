using System;

public abstract class AGenericBaseState<U, T, K> : IGenericBaseState<U>
    where U : GenericStateMachine<U, T, K>
    where K : AGenericBaseState<U, T, K> {
    [NonSerialized] private U _stateMachine;
    protected U StateMachine => _stateMachine;

    [NonSerialized] private T _stateMachineContext;

    protected T StateMachineContext => _stateMachineContext;

    public virtual void InitState(U stateMachine) {
        _stateMachine = stateMachine;
        _stateMachineContext = stateMachine.StateMachineContext;

        Init();
    }

    protected virtual void Init() { }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}