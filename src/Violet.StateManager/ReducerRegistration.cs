namespace Violet.StateManager;

public class ReducerRegistration<TState> : IReducerRegistration<TState>
{
    // object since cannot be typed perfectly
    private List<StateReducer> _reducers = new();
    public IEnumerable<StateReducer> Reducers => _reducers;
    private List<StateEffect> _effects = new();
    public IEnumerable<StateEffect> Effects => _effects;
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<TState>> function)
        where TAction : class
    {
        _reducers.Add(new AsyncStateReducer<TState, TAction>(function));
        return this;
    }
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, TState> function)
        where TAction : class
    {
        _reducers.Add(new SyncStateReducer<TState, TAction>(function));
        return this;
    }
    public IReducerRegistration<TState> OnChange(Func<TState, object, Task> function)
    {
        _effects.Add(new AsyncEffect<TState>(function));
        return this;
    }
    public IReducerRegistration<TState> OnChange(Action<TState, object> function)
    {
        _effects.Add(new SyncEffect<TState>(function));

        return this;
    }
}
