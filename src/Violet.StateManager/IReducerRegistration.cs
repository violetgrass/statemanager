namespace Violet.StateManager;
public interface IReducerRegistration<TState>
{
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<Result<TState>>> function)
        where TAction : class;
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<(TState, Error[])>> function)
        where TAction : class;
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Task<TState>> function)
        where TAction : class;
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, Result<TState>> function)
        where TAction : class;
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, (TState, Error[])> function)
        where TAction : class;
    IReducerRegistration<TState> On<TAction>(Func<TState, TAction, TState> function)
        where TAction : class;
    IReducerRegistration<TState> OnChange(Func<TState, object, Task> function);
    IReducerRegistration<TState> OnChange(Action<TState, object> function);

    IEnumerable<StateReducer> Reducers { get; }
    IEnumerable<StateEffect> Effects { get; }
}
