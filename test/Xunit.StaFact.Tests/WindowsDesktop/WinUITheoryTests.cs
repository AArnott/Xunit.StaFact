// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using Microsoft.UI.Dispatching;

public class WinUITheoryTests
{
    [WinUITheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WinUITheory_OnSTAThread(int arg)
    {
        Assert.IsType<DispatcherQueueSynchronizationContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        Assert.IsType<DispatcherQueueSynchronizationContext>(SynchronizationContext.Current);
        Assert.True(arg == 0 || arg == 1);
    }
}

#endif
