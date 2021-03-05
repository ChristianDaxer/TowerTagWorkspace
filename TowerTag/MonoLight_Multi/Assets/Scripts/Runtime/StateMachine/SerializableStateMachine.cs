using System.Collections.Generic;

/// <summary>
/// Generic State Machine which can Serialize its internal state (and that of the currentState) to a BitSerializer-Stream.
/// This can be used to synchronize this StateMachine over network.
/// You only have to call Serialize:
///         - on the Master client (with a writing stream):          when dirtyFlag is set
///         - on remote clients (with a reading stream):            when new data arrived
///         
/// Good to know:
///         - when calling ChangeState (on Master) the old state exits & the new state enters and the dirty flag is set, you then have to call Serialize with a write stream (see above)
///             - when the Serialize function of the NEW state is called and serializes the internal data of the state (which can be initialized in the EnterState() function)
///         - when the serialized data arrives at the remote clients call Serialize with a reading stream (with the serialized data in it)
///             - Serialize then exits the old state, deserializes the send data to the new state and then enters the new state so you can use the synced data in the EnterState() function
///             
///         -> in short:
///             - we ensure that the data of a state (on the remote clients) is valid before EnterState() of the new state is called
///             - you can init/use the internal data in EnterState() (on Master and remote clients)
///         
/// </summary>
/// <typeparam name="U">Type of StateMachine u want to derive from this class (so state machine object set in states has the right type).</typeparam>
/// <typeparam name="T">Type of StateMachineContext u want to use this StateMachine with (so StateMachineContext object set in states has the right type)</typeparam>
/// <typeparam name="K">Type of State u want to use this StateMachine with (so states have the right type)</typeparam>
public class SerializableStateMachine<U, T, K> : GenericStateMachine<U, T, K>
    where U : SerializableStateMachine<U, T, K>
    where K : SerializableState<U, T, K> {
    /// <summary>
    /// Cached number of states (only for convenience, used for BitCompression as upper value for IDs).
    /// </summary>
    private int _stateCount;

    /// <summary>
    /// available States for this state machine, used to find right State when Deserializing (ID -> state)
    /// </summary>
    private K[] _availableStates;

    /// <summary>
    /// Dirty flag is set to true if something changed and we need to synchronize.
    /// </summary>
    public bool IsDirty { get; set; }

    #region Block & queue incoming StateChanges

    /// <summary>
    /// If IsBlocked is true, Incoming State changes on remote clients (by Serialize(readStream) are queued.
    /// </summary>
    protected bool IsBlocked { get; set; }

    protected Queue<BitSerializer> BlockedIncomingSerializationQueue { get; } = new Queue<BitSerializer>();

    protected Queue<K> DebugBlockedIncomingSerializationQueue { get; } = new Queue<K>();

    #endregion

    /// <summary>
    /// Set available States for ths StateMachine & define their IDs (to send over network).
    /// Attention: the order of the states in the array should be the same because we use the index as ID!!!
    /// </summary>
    /// <param name="availableStates">States we use in this StateMachine.</param>
    protected void SetStates(K[] availableStates) {
        // check null
        if (availableStates == null) {
            Debug.LogError("Cannot set states to null");
            return;
        }

        // cache states so we can choose the right one when we receive an ID on the remote clients
        _availableStates = availableStates;

        // cache number of states (only for convenience -> used for BitCompression upper value)
        _stateCount = availableStates.Length;

        // define IDs for the states (simple version -> just use the array index, should be in the same order on every client!!!!!)
        for (var i = 0; i < availableStates.Length; i++) {
            _availableStates[i].StateID = i;
        }
    }

    /// <summary>
    /// Serialize/Deserialize current State to sync with remote StateMachine.
    /// If state machine is blocked (see isBlocked & BlockIncomingSerialization(block)), incoming calls (with readingStream) will get queued.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool Serialize(BitSerializer stream) {


        // write: Serialize on Master (and send to client)
        if (stream.IsWriting) {
            // check if we can sync
            if (CurrentState == null) {
                Debug.LogError("Current State is null");
                return false;
            }

            // reset dirty flag so we don't sync if nothing changes
            IsDirty = false;
            // write ID of current State to stream
            int currentStateID = CurrentState.StateID;
            stream.WriteInt(currentStateID, -1, _stateCount);

            // write ID of last State to stream
            int lastStateID = LastState?.StateID ?? -1;
            stream.WriteInt(lastStateID, -1, _stateCount);

            // serialize state to stream
            return CurrentState.Serialize(stream);
        }

        // read: DeSerialize on remote clients

        if (stream.IsReading) {
            if (IsBlocked) {
                // Enqueue complete copy of current stream
                BlockedIncomingSerializationQueue.Enqueue(stream.Clone());

                // save send state for debugging purpose
                int tmpStateID = stream.ReadInt(-1, _stateCount);
                K state = GetStateByID(tmpStateID);
                DebugBlockedIncomingSerializationQueue.Enqueue(state);

                return true;
            }

            // read current (master client) state from stream
            int newStateID = stream.ReadInt(-1, _stateCount);
            // read last (master client) state from stream
            int lastStateID = stream.ReadInt(-1, _stateCount);
            // set last state so independent what state we had (on local client) before
            // we have now the same configuration (currentState/lastState) as the master client
            LastState = lastStateID == -1 ? null : GetStateByID(lastStateID);

            // if we are in another state then the one that was send
            // -> Change State & Deserialize
            if (CurrentState == null || newStateID != CurrentState.StateID) {
                // set new state & Serialize
                K newState = GetStateByID(newStateID);
                bool success = newState.Serialize(stream);

                // enter new state after it was initialized successful
                if (success) {
                    ChangeState(newState);
                }

                return success;
            }

            // if state is the same deserialize the state
            return CurrentState.Serialize(stream);
        }

        return false;
    }

    /// <summary>
    /// Change State (called directly on master client, indirectly by ChangeState(ID) by remote clients).
    /// </summary>
    /// <param name="newState">The state we want to switch to.</param>
    protected override void ChangeState(K newState) {
        base.ChangeState(newState);
        IsDirty = true;
        if (CurrentState == null) Debug.LogError("Current state is null after changing state");
    }

    /// <summary>
    /// Find Serializable State with given ID
    /// </summary>
    /// <param name="id">ID of the Serializable State.</param>
    /// <returns>Serializable State with given ID if present, null otherwise.</returns>
    private K GetStateByID(int id) {
        if (id < 0 || id >= _availableStates.Length) {
            Debug.LogError("Invalid state id");
            return null;
        }

        return _availableStates[id];
    }

    /// <summary>
    /// Should incoming Serialization(isReading) calls get queued?
    /// -> see Serialize
    /// </summary>
    /// <param name="block">If true incoming Serialization(isReading) calls get queued, if false queued calls will be processed.</param>
    public void BlockIncomingSerialization(bool block) {
        if (IsBlocked == block) return;

        IsBlocked = block;

        // dequeue StateChanges (which came in when StateMachine was blocked)
        if (!IsBlocked) {
            // cache current size so we don't end up in an endless loop if a queued state blocks again (and therefore adds all subsequent calls back to the queue)
            int count = BlockedIncomingSerializationQueue.Count;
            while (count > 0) {
                // print queued debugInfo to console -> just for debugging
                DebugBlockedIncomingSerializationQueue.Dequeue();

                // dequeue state and call Serialize on it (again)
                BitSerializer stream = BlockedIncomingSerializationQueue.Dequeue();
                count--;

                if (stream != null) {
                    Serialize(stream);
                }
                else {
                    Debug.LogError("Cannot serialize stream: stream is null");
                }
            }
        }
    }
}