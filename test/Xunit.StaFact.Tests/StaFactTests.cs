﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using DesktopFactAttribute = Xunit.StaFactAttribute;

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

    [StaFact, Trait("TestCategory", "FailureExpected")]
    public void JustFailVoid() => throw new InvalidOperationException("Expected failure.");

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded()
    {
        if (MaxAttemptsHelper.GetAndIncrementAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name) != 1)
        {
            Assert.Fail("The first attempt false, but a second attempt will pass.");
        }
    }

    [DesktopFact]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded()
    {
        if (MaxAttemptsHelper.GetAndIncrementAttemptNumber(this.GetType(), MethodBase.GetCurrentMethod()!.Name) != 0)
        {
            Assert.Fail("This test should not have run a second time because the first run was successful.");
        }
    }

    [DesktopFact, Trait("TestCategory", "FailureExpected")]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries()
    {
        Assert.Fail("Failure expected.");
    }
}
