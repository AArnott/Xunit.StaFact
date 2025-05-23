// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using DesktopTheoryAttribute = Xunit.UITheoryAttribute;

public class UITheoryTests : IDisposable, IAsyncLifetime
{
    private readonly SynchronizationContext? ctorSyncContext;
    private readonly int ctorThreadId;

    public UITheoryTests()
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

    [DesktopTheory]
    [InlineData(0)]
    public void CtorAndTestMethodInvokedInSameContext(int arg)
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.Equal(0, arg);
    }

    [DesktopTheory]
    [InlineData(0)]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields(int arg)
    {
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.Equal(0, arg);
    }

    [DesktopTheory]
    [InlineData(0)]
    public async Task PassAfterYield(int arg)
    {
        // This will post to the SynchronizationContext before yielding.
        await Task.Yield();
        Assert.Equal(0, arg);
    }

    [DesktopTheory]
    [InlineData(0)]
    public async Task PassAfterDelay(int arg)
    {
        // This won't post to the SynchronizationContext till after the delay.
        await Task.Delay(10);
        Assert.Equal(0, arg);
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterYield(int arg)
    {
        await Task.Yield();
        Assert.Equal(1, arg);
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterDelay(int arg)
    {
        await Task.Delay(10);
        Assert.Equal(1, arg);
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterYield_Task(int arg)
    {
        await Task.Yield();
        Assert.Equal(1, arg);
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterDelay_Task(int arg)
    {
        await Task.Delay(10);
        Assert.Equal(1, arg);
    }

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task UITheory_OnSingleThreadedSyncContext(int arg)
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.True(arg == 0 || arg == 1);
    }

    [Trait("TestCategory", "FailureExpected")]
    [DesktopTheory]
    [InlineData(1)]
    public async Task UITheoryFails(int arg)
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.False(arg == 0 || arg == 1);
    }

    [DesktopTheory]
    [MemberData(nameof(NonSerializableObject.Data), MemberType = typeof(NonSerializableObject))]
    public void ThreadAffinitizedDataObject(NonSerializableObject o)
    {
        Assert.Equal(System.Diagnostics.Process.GetCurrentProcess().Id, o.ProcessId);
        Assert.Equal(Environment.CurrentManagedThreadId, o.ThreadId);
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public void JustFailVoid(int a) => throw new InvalidOperationException("Expected failure " + a);

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded(int arg) => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), $"{MethodBase.GetCurrentMethod()!.Name}_{arg}", 2);

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded(int arg) => MaxAttemptsHelper.ThrowUnlessAttemptNumber(this.GetType(), $"{MethodBase.GetCurrentMethod()!.Name}_{arg}", 1);

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries(int arg)
    {
        _ = arg;
        Assert.Fail("Failure expected.");
    }

    [DesktopTheory(SkipExceptions = [typeof(SkipOnThisException)])]
    [InlineData(0)]
    public void CanSkipOnSpecificExceptions(int arg)
    {
        _ = arg;
        throw new SkipOnThisException();
    }
}
