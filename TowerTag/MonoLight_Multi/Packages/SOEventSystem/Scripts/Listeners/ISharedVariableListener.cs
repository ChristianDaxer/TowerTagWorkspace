using SOEventSystem.Shared;

namespace SOEventSystem.Listeners {
    /// <summary>
    /// A <see cref="SharedVariable{T}"/> can register and unregister at a <see cref="ISharedVariableListener"/>
    /// so that in turn the <see cref="ISharedVariableListener"/> will register as a Set/Change listener.
    ///
    /// This interface is needed to have listeners that are agnostic to the underlying type of the
    /// <see cref="SharedVariable{T}"/> they are listening to.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public interface ISharedVariableListener {
        void ListenTo<T>(SharedVariable<T> variable);
        void StopListeningTo<T>(SharedVariable<T> sharedVariable);
    }
}