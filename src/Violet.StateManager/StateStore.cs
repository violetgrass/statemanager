using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Reflection.PortableExecutable;

using Microsoft.Extensions.Logging;

namespace Violet.StateManager;

public partial class StateStore<TState>
    where TState : class
{
    protected TState _state;
    protected readonly ILogger? _logger;

    public TState State => _state;

    private readonly BehaviorSubject<TState> _observable;
    public IObservable<TState> Observable => _observable;

    private readonly ConcurrentQueue<object> _actionQueue = new();
    private bool _isDispatching = false;

    public StateStore(TState initialState, ILogger? logger = null)
    {
        _state = initialState;
        _logger = logger;
        _observable = new BehaviorSubject<TState>(_state);
        Registration = new ReducerRegistration<TState>();
        Dispatcher = new Dispatcher<TState>(Registration, logger: logger);
    }

    public IReducerRegistration<TState> Registration { get; }
    public Dispatcher<TState> Dispatcher { get; }

    public async Task<TState> DispatchAsync<TAction>(TAction action)
        where TAction : class
        => await DispatchAsync((object)action);

    public async Task<TState> DispatchAsync(params object[] actions)
    {
        TState result = _state;

        foreach (var action in actions)
        {
            result = await DispatchAsync(action);
        }

        return result;
    }

    public async Task<TState> DispatchAsync(object action)
    {
        _logger?.LogInformation($"Enqueue action {action.GetType().Name}");
        _actionQueue.Enqueue(action);

        if (!_isDispatching)
        {
            _isDispatching = true;

            while (_actionQueue.TryDequeue(out var currentAction))
            {
                _logger?.LogInformation($"Begin Dispatch {currentAction.GetType().Name}");

                _state = await Dispatcher.DispatchAsync(_state, currentAction);

                _logger?.LogInformation($"End Dispatch {currentAction.GetType().Name}");
            }

            _isDispatching = false;

            _logger?.LogInformation("Announce on Observable");
            _observable.OnNext(_state);
        }


        return _state;
    }

    public void SubState<TSubState, TKey>(
        Func<object, TKey> keySelector,
        Func<TState, TKey, TSubState?> subStateSelector,
        Func<TState, TKey, TSubState, Task<TState>> subStateReducer,
        IReducerRegistration<TSubState> registration)
        where TSubState : class
    {
        var dispatcher = new Dispatcher<TSubState>(registration, a => keySelector(a), (s, k) => subStateSelector((s as TState)!, (TKey)k)!, async (s, k, ss) => await subStateReducer((s as TState)!, (TKey)k, (ss as TSubState)!), _logger);

        Dispatcher.SubDispatchers.Add(dispatcher);
    }

    public void SubState<TSubState, TKey>(
        Func<object, TKey> keySelector,
        Func<TState, TKey, TSubState?> subStateSelector,
        Func<TState, TKey, TSubState, TState> subStateReducer,
        IReducerRegistration<TSubState> registration)
        where TSubState : class
        => SubState<TSubState, TKey>(keySelector, subStateSelector, (state, key, subState) => Task.FromResult(subStateReducer(State, key, subState)), registration);

    public IReducerRegistration<TSubState> SubState<TSubState, TKey>(
        Func<object, TKey> keySelector,
        Func<TState, TKey, TSubState?> subStateSelector,
        Func<TState, TKey, TSubState, TState> subStateReducer)
        where TSubState : class
        => SubState(keySelector, subStateSelector, (state, key, subState) => Task.FromResult(subStateReducer(State, key, subState)));

    public IReducerRegistration<TSubState> SubState<TSubState, TKey>(
        Func<object, TKey> keySelector,
        Func<TState, TKey, TSubState?> subStateSelector,
        Func<TState, TKey, TSubState, Task<TState>> subStateReducer)
        where TSubState : class
    {
        var reducers = new ReducerRegistration<TSubState>();
        var dispatcher = new Dispatcher<TSubState>(reducers, a => keySelector(a), (s, k) => subStateSelector((s as TState)!, (TKey)k), async (s, k, ss) => await subStateReducer((s as TState)!, (TKey)k, (ss as TSubState)!), _logger);

        Dispatcher.SubDispatchers.Add(dispatcher);

        return reducers;
    }
}
