/// <summary>
/// Abstract base class for states used with Serializable State Machine.
/// </summary>
/// <typeparam name="U">Type of StateMachine u want to derive from (so state machine object is set to the right type).</typeparam>
/// <typeparam name="T">Type of StateMachineContext u want to use (so state machineContext object is set to the right type)</typeparam>
/// <typeparam name="K">Type of State (base class) u want to use.</typeparam>
public abstract class SerializableState<U, T, K> : AGenericBaseState<U, T, K>
    where U : SerializableStateMachine<U, T, K>
    where K : SerializableState<U, T, K>

{
    /// <summary>
    /// Unique ID used for this state (IDs only unique per StateMachine).
    /// </summary>
    public int StateID { get; set; }

    /// <summary>
    /// Please implement synchronisation of internal state here.
    /// </summary>
    /// <param name="stream">Stream to read from or write your data to.</param>
    /// <returns>True if succeeded read/write, false otherwise.</returns>
    public abstract bool Serialize(BitSerializer stream);
}
