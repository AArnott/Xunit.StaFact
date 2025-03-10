// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using DesktopFactAttribute = Xunit.UIFactAttribute;

public partial class UIFactTests : IDisposable, IAsyncLifetime
{
    private readonly SynchronizationContext? ctorSyncContext;
    private readonly int ctorThreadId;

    public UIFactTests()
    {
        this.ctorSyncContext = SynchronizationContext.Current;
        this.ctorThreadId = Environment.CurrentManagedThreadId;
        Assert.NotNull(this.ctorSyncContext);
    }

    public void Dispose()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public async ValueTask InitializeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public async ValueTask DisposeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public void CtorAndTestMethodInvokedInSameContext()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields()
    {
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public async Task PassAfterYield()
    {
        // This will post to the SynchronizationContext before yielding.
        await Task.Yield();
    }

    [DesktopFact]
    public async Task PassAfterDelay()
    {
        // This won't post to the SynchronizationContext till after the delay.
        await Task.Delay(10);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield_Task()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay_Task()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [DesktopFact]
    public async Task UIFact_OnSingleThreadedSyncContext()
    {
        int initialThread = Environment.CurrentManagedThreadId;
        SynchronizationContext? syncContext = SynchronizationContext.Current;
        await Task.Yield();
        Assert.Equal(initialThread, Environment.CurrentManagedThreadId);
        Assert.Same(syncContext, SynchronizationContext.Current);
    }

    [DesktopFact]
    public async Task SendBackFromOtherThread()
    {
        SynchronizationContext sc = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext");
        bool delegateComplete = false;
        await Task.Run(delegate
        {
            sc.Send(
                s =>
                {
                    Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
                    Assert.Equal(5, (int)s!);
                },
                5);
            delegateComplete = true;
        });
        Assert.True(delegateComplete);
    }

    [DesktopFact]
    public async Task SendBackFromOtherThread_Throws()
    {
        SynchronizationContext sc = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext");
        await Task.Run(delegate
        {
            Assert.Throws<System.IO.IOException>(() =>
                sc.Send(
                    s =>
                    {
                        throw new System.IO.IOException();
                    },
                    5));
        });
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    public void JustFailVoid() => throw new InvalidOperationException("Expected failure.");

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 2);

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 1);

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries()
    {
        Assert.Fail("Failure expected.");
    }

    [DesktopFact(SkipExceptions = [typeof(SkipOnThisException)])]
    public void CanSkipOnSpecificExceptions()
    {
        throw new SkipOnThisException();
    }

    [UISettings(MaxAttempts = 2)]
    public class ClassWithDefaultRetryPolicy
    {
        [DesktopFact]
        public void AutomaticRetryNeeded() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 2);

        [DesktopFact, Trait("TestCategory", "FailureExpected")]
        public void FailsAllRetries()
        {
            Assert.Fail("Failure expected.");
        }

        [DesktopFact]
        [UISettings(MaxAttempts = 3)]
        public void SucceedOn3rdAttempt() => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name, 3);
    }
}
