namespace Violet.StateManager.Tests;

public class StateManagerTest
{
    public record MyState(string A, string B);
    public record DeleteA();

    [Fact]
    public async Task StateManager_Reducer_Simple()
    {
        // arrange
        var state = new StateStore<MyState>(new MyState("a", "b"));
        state.Registration.On<DeleteA>(async (state, action) => state with { A = string.Empty });

        // act
        var actual = await state.DispatchAsync(new DeleteA());

        // assert
        Assert.Equal(string.Empty, actual.A);
        Assert.Equal("b", actual.B);
    }


    [Fact]
    public async Task StateManager_Effect_Simple()
    {
        // arrange
        var state = new StateStore<MyState>(new MyState("a", "b"));
        var called = false;
        state.Registration.On<DeleteA>(async (state, action) => state with { A = string.Empty });
        state.Registration.OnChange(async (state, action) => called = true);

        // act
        var actual = await state.DispatchAsync(new DeleteA());

        // assert
        Assert.Equal(string.Empty, actual.A);
        Assert.Equal("b", actual.B);
        Assert.True(called);
    }
}