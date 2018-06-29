// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if !NET45

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xunit;

public class WpfTheoryTests
{
    [WpfTheory]
    [InlineData(0)]
    public async Task WpfTheory_OnSTAThread(int zero)
    {
        Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
        Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
        Assert.Equal(0, zero);
    }
}

#endif