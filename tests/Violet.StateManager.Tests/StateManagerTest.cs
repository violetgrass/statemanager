namespace Violet.StateManager.Tests;

public class StateManagerTest
{
    public record MyState(string A, string B);
    public record DeleteA();
    public record SetB(string X);

    [Fact]
    public async Task StateManager_Reducer_Simple()
    {
        // arrange
        var state = new StateStore<MyState>(new MyState("a", "b"));
        state.Registration.On<DeleteA>((state, action) => state with { A = string.Empty });

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
        state.Registration.On<DeleteA>((state, action) => state with { A = string.Empty });
        state.Registration.OnChange((state, action) => called = true);

        // act
        var actual = await state.DispatchAsync(new DeleteA());

        // assert
        Assert.Equal(string.Empty, actual.A);
        Assert.Equal("b", actual.B);
        Assert.True(called);
    }

    [Fact]
    public async Task StateManager_DispatchAsync_Multiple()
    {
        // arrange
        var state = new StateStore<MyState>(new MyState("a", "b"));
        state.Registration.On<DeleteA>((state, action) => state with { A = string.Empty });
        state.Registration.On<SetB>((state, action) => state with { B = action.X });

        // act
        var actual = await state.DispatchAsync(
            new SetB("x1"),
            new DeleteA(),
            new SetB("x2")
        );

        // assert
        Assert.Equal(string.Empty, actual.A);
        Assert.Equal("x2", actual.B);
    }
}