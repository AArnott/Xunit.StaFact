// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable xUnit1008

public class UITheoryTests : IDisposable, IAsyncLifetime
{
    private readonly SynchronizationContext ctorSyncContext;
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

    public async Task InitializeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public async Task DisposeAsync()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [UITheory]
    [InlineData(0)]
    public void CtorAndTestMethodInvokedInSameContext(int arg)
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.Equal(0, arg);
    }

    [UITheory]
    [InlineData(0)]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields(int arg)
    {
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
        Assert.Equal(0, arg);
    }

    [UITheory]
    [InlineData(0)]
    public async void PassAfterYield(int arg)
    {
        // This will post to the SynchronizationContext before yielding.
        await Task.Yield();
        Assert.Equal(0, arg);
    }

    [UITheory]
    [InlineData(0)]
    public async void PassAfterDelay(int arg)
    {
        // This won't post to the SynchronizationContext till after the delay.
        await Task.Delay(10);
        Assert.Equal(0, arg);
    }

    [UITheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async void FailAfterYield(int arg)
    {
        await Task.Yield();
        Assert.Equal(1, arg);
    }

    [UITheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async void FailAfterDelay(int arg)
    {
        await Task.Delay(10);
        Assert.Equal(1, arg);
    }

    [UITheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterYield_Task(int arg)
    {
        await Task.Yield();
        Assert.Equal(1, arg);
    }

    [UITheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public async Task FailAfterDelay_Task(int arg)
    {
        await Task.Delay(10);
        Assert.Equal(1, arg);
    }

    [UITheory]
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
    [UITheory]
    [InlineData(0)]
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
}
