// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xunit;

public partial class Samples
{
    [Fact]
    public async Task Fact_OnMTAThread()
    {
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
    }

    [WpfFact]
    public async Task WpfFact_OnSTAThread()
    {
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
    }

    [StaFact]
    public async Task StaWithoutSyncContext()
    {
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();

        // Without a single-threaded SynchronizationContext, we won't come back to the STA thread.
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());
    }
}
