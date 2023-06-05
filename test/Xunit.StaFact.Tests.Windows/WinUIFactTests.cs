// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;
using DesktopFactAttribute = Xunit.WinUIFactAttribute;
using DesktopSyncContext = Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext;

/// <summary>
/// Verifies behavior of the <see cref="WinFormsFactAttribute"/>.
/// </summary>
/// <remarks>
/// The members of this class should be kept in exact sync with those of the
/// WinFormsFactTests.cs file.
/// </remarks>
public class WinUIFactTests
{
    private readonly Thread ctorThread;
    private readonly SynchronizationContext? ctorSyncContext;

    public WinUIFactTests()
    {
        this.ctorThread = Thread.CurrentThread;
        this.ctorSyncContext = SynchronizationContext.Current;
    }

    [DesktopFact]
    public void Void()
    {
        this.AssertThreadCharacteristics();
    }

    [DesktopFact]
    public async Task AsyncTask()
    {
        this.AssertThreadCharacteristics();
        await Task.Yield();
        this.AssertThreadCharacteristics();
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void AsyncVoid_IsNotSupported()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield_Task()
    {
        // Task.Yield posts a message immediately (before yielding)
        await Task.Yield();
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay_Task()
    {
        // Task.Delay waits for the elapsed time after yielding before posting a message.
        await Task.Delay(10);
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task OperationCanceledException_Thrown()
    {
        await Task.Yield();
        throw new OperationCanceledException();
    }

    [DesktopFact]
    public void ShouldShowWindow()
    {
        var window = new Window();
        window.Activate();

        Assert.True(window.Visible);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public void JustFailVoid() => throw new InvalidOperationException("Expected failure.");

    private void AssertThreadCharacteristics()
    {
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.IsType<DesktopSyncContext>(SynchronizationContext.Current);

        Assert.Same(this.ctorThread, Thread.CurrentThread);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }
}
