using Renderite.Shared;
using System.Runtime.CompilerServices;

namespace Renderite.Unity.Tests;

[TestClass]
public sealed class MouseInputTests
{
    [TestMethod]
    public void UpdateState_MouseStateProvided_UpdatesWithExistingMouseState()
    {
        MouseState expectedMouseState = new();
        var mockMouseInput = (TestMouseInput)RuntimeHelpers.GetUninitializedObject(typeof(TestMouseInput));

        InputState mockInputState = new()
        {
            mouse = expectedMouseState
        };

        mockMouseInput.UpdateState(mockInputState);

        Assert.AreEqual(1, mockMouseInput.UpdateStateCallCount);
        Assert.AreEqual(expectedMouseState, mockMouseInput.ActualMouseState);
    }

    [TestMethod]
    public void UpdateState_MouseStateIsNull_UpdatesWithNewMouseState()
    {
        var mockMouseInput = (TestMouseInput)RuntimeHelpers.GetUninitializedObject(typeof(TestMouseInput));

        InputState mockInputState = new()
        {
            // This is awkward; however, MouseInput has a null check, so it has to be done.
            mouse = null!
        };
        Assert.IsNull(mockMouseInput.ActualMouseState);

        mockMouseInput.UpdateState(mockInputState);

        Assert.AreEqual(1, mockMouseInput.UpdateStateCallCount);
        Assert.IsNotNull(mockMouseInput.ActualMouseState);
    }
}

file sealed class TestMouseInput : MouseInput
{
    public int UpdateStateCallCount { get; private set; }

    public int HandleStateUpdateCallCount { get; private set; }

    public MouseState? ActualMouseState { get; private set; }

    public OutputState? ActualOutputState { get; private set; }

    public override void HandleStateUpdate(OutputState state)
    {
        HandleStateUpdateCallCount++;
        ActualOutputState = state;
    }

    protected override void UpdateState(MouseState state)
    {
        UpdateStateCallCount++;
        ActualMouseState = state;
    }
}