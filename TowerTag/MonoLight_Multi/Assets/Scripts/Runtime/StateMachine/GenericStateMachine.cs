/// <summary>
/// Generic State Machine (use this as base class)
/// </summary>
/// <typeparam name="U">Type of StateMachine u want to derive from this class (so state machine object set in states has the right type).</typeparam>
/// <typeparam name="T">Type of StateMachineContext u want to use this StateMachine with (so StateMachineContext object set in states has the right type)</typeparam>
/// <typeparam name="K">Type of State u want to use this StateMachine with (so states set have the right type)</typeparam>
[System.Serializable]
public class GenericStateMachine<U, T, K>  where K : IGenericBaseState<U>
{
    /// <summary>
    /// Signature of stateChangedEvent.
    /// </summary>
    /// <param name="oldState">The old state that was active before the Statechange.</param>
    /// <param name="newState">The new state we switched to.</param>
    public delegate void StateChangedDelegate(K oldState, K newState);

    /// <summary>
    /// StateChangedEvent: is called when the state was changed (after the old state exited and the new state entered)
    /// </summary>
    public event StateChangedDelegate StateChanged;

    [System.NonSerialized] private T _stateMachineContext;

    /// <summary>
    /// The Context all states can access to communicate with the environment(get values or call functions).
    /// </summary>
    public T StateMachineContext => _stateMachineContext;

    /// <summary>
    /// The state we are in at the moment.
    /// </summary>
    protected K CurrentState { get; private set; }

    /// <summary>
    /// The last state we were in before we switched to the current state.
    /// </summary>
    protected K LastState { get; set; }

    /// <summary>
    /// Init the state machine: save the context.
    /// </summary>
    /// <param name="stateMachineContext">The Context all states can access to communicate with the environment (get values or call functions).</param>
    public virtual void InitStateMachine(T stateMachineContext)
    {
        _stateMachineContext = stateMachineContext;
    }

    /// <summary>
    /// Calls UpdateState on the current state (if set).
    /// </summary>
    public void UpdateCurrentState()
    {
        if (CurrentState != null)
        {
            CurrentState.UpdateState();
        }
    }

    /// <summary>
    /// Change the state of the state machine.
    /// </summary>
    /// <param name="newState">The new state to switch to.</param>
    protected virtual void ChangeState(K newState)
    {
        LastState = CurrentState;

        if (CurrentState != null)
        {
            CurrentState.ExitState();
        }

        CurrentState = newState;

        if (CurrentState != null)
        {
            CurrentState.EnterState();
        }

        StateChanged?.Invoke(LastState, newState);
    }
}