// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using System.Reflection;
using Microsoft.UI.Dispatching;
using WinUIButton = Microsoft.UI.Xaml.Controls.Button;
using WinUIWindow = Microsoft.UI.Xaml.Window;

public class WinUIFactTests
{
    private readonly Thread ctorThread;
    private readonly SynchronizationContext? ctorSyncContext;

    public WinUIFactTests()
    {
        this.ctorThread = Thread.CurrentThread;
        this.ctorSyncContext = SynchronizationContext.Current;
    }

    [WinUIFact]
    public void Void()
    {
        this.AssertThreadCharacteristics();
    }

    [WinUIFact]
    public async Task AsyncTask()
    {
        this.AssertThreadCharacteristics();
        await Task.Yield();
        this.AssertThreadCharacteristics();
    }

    [WinUIFact]
    public void CanCreateControlsAndWindows()
    {
        var button = new WinUIButton();
        var window = new WinUIWindow();

        Assert.True(button.DispatcherQueue.HasThreadAccess);
        Assert.True(window.DispatcherQueue.HasThreadAccess);
    }

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void AsyncVoid_IsNotSupported()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
    }

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield_Task()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay_Task()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
    public async Task OperationCanceledException_Thrown()
    {
        await Task.Yield();
        throw new OperationCanceledException();
    }

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
    public void JustFailVoid() => throw new InvalidOperationException("Expected failure.");

    [WinUIFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 2);

    [WinUIFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 1);

    [WinUIFact, Trait("TestCategory", "FailureExpected")]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries()
    {
        Assert.Fail("Failure expected.");
    }

    [WinUIFact(SkipExceptions = [typeof(SkipOnThisException)])]
    public void CanSkipOnSpecificExceptions()
    {
        throw new SkipOnThisException();
    }

    private void AssertThreadCharacteristics()
    {
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.IsType<DispatcherQueueSynchronizationContext>(SynchronizationContext.Current);
        Assert.Same(this.ctorThread, Thread.CurrentThread);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        Assert.True(DispatcherQueue.GetForCurrentThread().HasThreadAccess);
    }
}

#endif
