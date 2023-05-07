namespace Violet.StateManager;

public abstract record StateReducer(Predicate<object> ForAction);
public abstract record AsyncStateReducer<TState>(Predicate<object> ForAction, Func<TState, object, Task<TState>> FunctionInternalAsync)
    : StateReducer(ForAction);
public abstract record SyncStateReducer<TState>(Predicate<object> ForAction, Func<TState, object, TState> FunctionInternal)
    : StateReducer(ForAction);

public record AsyncStateReducer<TState, TAction>(Func<TState, TAction, Task<TState>> FunctionAsync)
    : AsyncStateReducer<TState>(action => action is TAction, async (state, action) => await FunctionAsync(state, action as TAction))
    where TAction : class;
public record SyncStateReducer<TState, TAction>(Func<TState, TAction, TState> Function)
    : SyncStateReducer<TState>(action => action is TAction, (state, action) => Function(state, action as TAction))
    where TAction : class;

public record StateEffect();
public record AsyncEffect<TState>(Func<TState, object, Task> FunctionAsync)
    : StateEffect();
public record SyncEffect<TState>(Action<TState, object> Function)
    : StateEffect();