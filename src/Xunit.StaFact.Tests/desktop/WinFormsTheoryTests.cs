// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DesktopSyncContext = System.Windows.Forms.WindowsFormsSynchronizationContext;
using DesktopTheoryAttribute = Xunit.WinFormsTheoryAttribute;

#pragma warning disable xUnit1008

public class WinFormsTheoryTests
{
    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WpfTheory_OnSTAThread(int arg)
    {
        Assert.IsType<DesktopSyncContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
        Assert.IsType<DesktopSyncContext>(SynchronizationContext.Current);
        Assert.True(arg == 0 || arg == 1);
    }

    [Trait("TestCategory", "FailureExpected")]
    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WpfTheoryFails(int arg)
    {
        Assert.IsType<DesktopSyncContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
        Assert.IsType<DesktopSyncContext>(SynchronizationContext.Current);
        Assert.False(arg == 0 || arg == 1);
    }
}
