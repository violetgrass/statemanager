using Microsoft.Extensions.Logging;

namespace Violet.StateManager;

public abstract class Dispatcher
{
    public abstract bool SupportsReducerForAction(object action);
    public abstract Task<object> DispatchAsync<TAction>(object state, TAction action)
        where TAction : class;
}


public class Dispatcher<TState> : Dispatcher
    where TState : class
{
    public List<Dispatcher> SubDispatchers { get; } = new();
    private Func<object, object?>? _keySelector = null;
    private Func<object, object, object>? _subStateSelector = null;
    private Func<object, object, object, Task<object>>? _subStateReducer = null;
    private readonly ILogger? _logger;

    public Dispatcher(IReducerRegistration<TState> registration, Func<object, object?>? keySelector = null, Func<object, object, object>? subStateSelector = null, Func<object, object, object, Task<object>>? subStateReducer = null, ILogger? logger = null)
    {
        Registration = registration;
        _keySelector = keySelector;
        _subStateSelector = subStateSelector;
        _subStateReducer = subStateReducer;
        _logger = logger;
    }

    public IReducerRegistration<TState> Registration { get; }

    public override async Task<object> DispatchAsync<TAction>(object state, TAction action)
        where TAction : class
    {
        object? currentState = null;
        object? key = null;

        _logger?.LogInformation($"Start dispatcher on state {typeof(TState)}");

        // retrieve state
        if (_keySelector is not null && _subStateSelector is not null)
        {
            key = _keySelector(action);
            if (key is not null)
            {
                currentState = _subStateSelector(state, key);
                _logger?.LogInformation($"Identified sub state {currentState?.GetType()?.Name} with key {key}");
            }
        }
        else
        {
            currentState = state;
        }

        if (currentState is not null)
        {
            // iterate sub dispatchers
            foreach (var subDispatcher in SubDispatchers)
            {
                _logger?.LogInformation($"Invoke sub dispatcher");

                currentState = await subDispatcher.DispatchAsync<TAction>(currentState, action);
            }

            // regular local reducer
            if (SupportsReducerForAction(action) || HasEffects())
            {
                currentState = await ReduceAsync<TState, TAction>(currentState as TState, action, Registration.Reducers);
            }
            else
            {
                _logger?.LogInformation($"No reducer found for state {typeof(TState).Name}");
            }

            if (HasEffects())
            {
                await EffectAsync(currentState as TState, action, Registration.Effects);
            }
            else
            {
                _logger?.LogInformation($"No effects found for state {typeof(TState).Name}");
            }

            // re-integrate state
            if (_subStateReducer is not null && key is not null)
            {
                state = await _subStateReducer(state, key, currentState);
                _logger?.LogInformation($"Reintegrated sub state {currentState?.GetType()?.Name} into {typeof(TState).Name}");
            }
            else
            {
                state = currentState;
            }
        }

        return state;
    }

    public async Task<TState> DispatchAsync<TAction>(TState state, TAction action)
        where TAction : class
        => await this.DispatchAsync<TAction>(state as object, action) as TState;

    public async Task<TCurrentState> ReduceAsync<TCurrentState, TAction>(TCurrentState state, TAction action, IEnumerable<StateReducer> reducers)
        where TAction : class
    {
        var intermediateState = state;
        var t = action.GetType();

        _logger?.LogInformation($"Found {reducers.Count()} reducers for state {typeof(TState).Name}");

        foreach (var reducer in reducers)
        {
            if (reducer is AsyncStateReducer<TCurrentState> asyncReducer && asyncReducer.ForAction(action))
            {
                _logger?.LogInformation($"Invoking asynchronous Reducer");
                (intermediateState, _) = await asyncReducer.FunctionInternalAsync(intermediateState, action);
            }
            if (reducer is SyncStateReducer<TCurrentState> syncReducer && syncReducer.ForAction(action))
            {
                _logger?.LogInformation($"Invoking synchronous Reducer");
                (intermediateState, _) = syncReducer.FunctionInternal(intermediateState, action);
            }
        }

        state = intermediateState;

        return state;
    }

    private async Task EffectAsync<TCurrentState, TAction>(TCurrentState state, TAction action, IEnumerable<StateEffect> effects) where TAction : class
    {
        _logger?.LogInformation($"Found {effects.Count()} effects for state {typeof(TState).Name}");

        foreach (var effect in effects)
        {
            switch (effect)
            {
                case AsyncEffect<TCurrentState> asnycEffect:
                    _logger?.LogInformation($"Invoking asynchronous Effect");
                    await asnycEffect.FunctionAsync(state, action);
                    break;
                case SyncEffect<TCurrentState> syncEffect:
                    _logger?.LogInformation($"Invoking synchronous Effect");
                    syncEffect.Function(state, action);
                    break;
            }
        }
    }

    public override bool SupportsReducerForAction(object action)
        => Registration.Reducers.Any(r => r.ForAction(action));
    public bool HasEffects()
        => Registration.Effects.Any();
}