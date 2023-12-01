using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Violet.StateManager.Tests;

public record Action1();
public record SubAction1(int Key, string C, string D);

public record SampleState(string A, Dictionary<int, SampleSubState> B);
public record SampleSubState(string C, string D);

public class SampleSubStateStore : StateStore<SampleSubState>
{
    public SampleSubStateStore(SampleSubState initialState)
        : base(initialState)
    {
        Registration
            .On<SubAction1>(SubAction1Reducer);
    }
    public static SampleSubState SubAction1Reducer(SampleSubState s, SubAction1 a)
        => s with { C = a.C, D = a.D };
}

public class SampleStateStore : StateStore<SampleState>
{
    public SampleStateStore(SampleState initialState)
        : base(initialState)
    {
        Registration
            .On<Action1>(Action1Reducer);

        SubState<SampleSubState, int?>(
            action => (action as SubAction1)?.Key,
            (state, key) => state.B[key.Value],
            async (state, key, subState) => { state.B[key.Value] = subState; return state; },
            new SampleSubStateStore(null).Registration
        );
    }

    public static SampleState Action1Reducer(SampleState s, Action1 a)
        => s with { A = "1" };
}


public class SubStateTest
{
    [Fact]
    public async Task StateStore_Setup_OnObject()
    {
        // arrange
        var stateStore = new StateStore<SampleState>(new SampleState("A", new Dictionary<int, SampleSubState>() { { 1, new SampleSubState("c0", "d0") } }));
        stateStore.Registration.On<Action1>((s, a) => s with { A = "1" });
        stateStore.SubState<SampleSubState, int?>(
            action => (action as SubAction1)?.Key,
            (state, key) => state.B[key.Value],
            async (state, key, subState) => { state.B[key.Value] = subState; return state; })
            .On<SubAction1>((s, a) => s with { C = a.C, D = a.D });

        // act
        await stateStore.DispatchAsync(new Action1());
        await stateStore.DispatchAsync(new SubAction1(1, "c1", "d1"));

        // assert
        Assert.Equal("1", stateStore.State.A);
        Assert.Equal("c1", stateStore.State.B[1].C);
    }

    [Fact]
    public async Task StateStore_Setup_OnDerivedClass()
    {
        // arrange
        var stateStore = new SampleStateStore(new SampleState("A", new Dictionary<int, SampleSubState>() { { 1, new SampleSubState("c0", "d0") } }));

        // act
        await stateStore.DispatchAsync(new Action1());
        await stateStore.DispatchAsync(new SubAction1(1, "c1", "d1"));

        // assert
        Assert.Equal("1", stateStore.State.A);
        Assert.Equal("c1", stateStore.State.B[1].C);
    }

}