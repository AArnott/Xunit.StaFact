// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public partial class UIFactTests : IDisposable, IAsyncLifetime
{
    private readonly SynchronizationContext ctorSyncContext;
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

    [UIFact]
    public void CtorAndTestMethodInvokedInSameContext()
    {
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [UIFact]
    public async Task CtorAndTestMethodInvokedInSameContext_AcrossYields()
    {
        await Task.Yield();
        Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    [UIFact]
    public async void PassAfterYield()
    {
        // This will post to the SynchronizationContext before yielding.
        await Task.Yield();
    }

    [UIFact]
    public async void PassAfterDelay()
    {
        // This won't post to the SynchronizationContext till after the delay.
        await Task.Delay(10);
    }

    [UIFact, Trait("TestCategory", "FailureExpected")]
    public async void FailAfterYield()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [UIFact, Trait("TestCategory", "FailureExpected")]
    public async void FailAfterDelay()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [UIFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterYield_Task()
    {
        await Task.Yield();
        Assert.False(true);
    }

    [UIFact, Trait("TestCategory", "FailureExpected")]
    public async Task FailAfterDelay_Task()
    {
        await Task.Delay(10);
        Assert.False(true);
    }

    [UIFact]
    public async Task UIFact_OnSingleThreadedSyncContext()
    {
        int initialThread = Environment.CurrentManagedThreadId;
        var syncContext = SynchronizationContext.Current;
        await Task.Yield();
        Assert.Equal(initialThread, Environment.CurrentManagedThreadId);
        Assert.Same(syncContext, SynchronizationContext.Current);
    }

    [UIFact]
    public async Task SendBackFromOtherThread()
    {
        var sc = SynchronizationContext.Current;
        bool delegateComplete = false;
        await Task.Run(delegate
        {
            sc.Send(
                s =>
                {
                    Assert.Equal(this.ctorThreadId, Environment.CurrentManagedThreadId);
                    Assert.Equal(5, (int)s);
                },
                5);
            delegateComplete = true;
        });
        Assert.True(delegateComplete);
    }

    [UIFact]
    public async Task SendBackFromOtherThread_Throws()
    {
        var sc = SynchronizationContext.Current;
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
}
