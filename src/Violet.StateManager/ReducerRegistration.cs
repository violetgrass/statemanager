namespace Violet.StateManager;

public class ReducerRegistration<TState> : IReducerRegistration<TState>
{
    // object since cannot be typed perfectly
    private List<StateReducer> _reducers = new();
    public IEnumerable<StateReducer> Reducers => _reducers;
    private List<StateEffect> _effects = new();
    public IEnumerable<StateEffect> Effects => _effects;
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<Result<TState>>> function)
        where TAction : class
    {
        _reducers.Add(new AsyncStateReducer<TState, TAction>(function));
        return this;
    }
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<(TState, Error[])>> function)
        where TAction : class
        => On<TAction>(async (state, action) => { var (newState, errors) = await function(state, action); return new Result<TState>(newState, errors); });
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<TState>> function)
        where TAction : class
        => On<TAction>(async (state, action) => new Result<TState>(await function(state, action), Array.Empty<Error>()));

    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Result<TState>> function)
        where TAction : class
    {
        _reducers.Add(new SyncStateReducer<TState, TAction>(function));
        return this;
    }
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, (TState, Error[])> function)
        where TAction : class
        => On<TAction>((state, action) => { var (newState, errors) = function(state, action); return new Result<TState>(newState, errors); });
    public IReducerRegistration<TState> On<TAction>(Func<TState, TAction, TState> function)
        where TAction : class
        => On<TAction>((state, action) => new Result<TState>(function(state, action), Array.Empty<Error>()));

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
