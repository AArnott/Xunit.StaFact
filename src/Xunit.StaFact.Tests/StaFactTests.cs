// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class StaFactTests
{
    public StaFactTests()
    {
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }

    [StaFact]
    public async Task StaWithoutSyncContext()
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();

        // Without a single-threaded SynchronizationContext, we won't come back to the STA thread.
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
    }

    [StaFact]
    public void Sta_Void()
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }

    [StaFact, Trait("TestCategory", "FailureExpected")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async void AsyncVoid_IsNotSupported()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        // this test should be rejected by test discovery because
        // async void tests aren't supportable when you have no SynchronizationContext.
    }
}
