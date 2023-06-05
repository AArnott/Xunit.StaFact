// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using DesktopTheoryAttribute = Xunit.WpfTheoryAttribute;

public class WinFormsTheoryTests
{
    [WinFormsTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WinFormsTheory_OnSTAThread(int arg)
    {
        Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
        Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
        Assert.True(arg == 0 || arg == 1);
    }

    [Trait("TestCategory", "FailureExpected")]
    [WinFormsTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task WinFormsTheoryFails(int arg)
    {
        Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
        Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
        Assert.False(arg == 0 || arg == 1);
    }

    [WinFormsTheory]
    [MemberData(nameof(NonSerializableObject.Data), MemberType = typeof(NonSerializableObject))]
    public void ThreadAffinitizedDataObject(NonSerializableObject o)
    {
        Assert.Equal(System.Diagnostics.Process.GetCurrentProcess().Id, o.ProcessId);
        Assert.Equal(Environment.CurrentManagedThreadId, o.ThreadId);
    }

    [WinFormsTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    public void JustFailVoid(int a) => throw new InvalidOperationException("Expected failure " + a);

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNeeded(int arg)
    {
        if (MaxAttemptsHelper.GetAndIncrementAttemptNumber(this.GetType(), $"{MethodBase.GetCurrentMethod()!.Name}_{arg}") != 1)
        {
            Assert.Fail("The first attempt false, but a second attempt will pass.");
        }
    }

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void AutomaticRetryNotNeeded(int arg)
    {
        if (MaxAttemptsHelper.GetAndIncrementAttemptNumber(this.GetType(), $"{MethodBase.GetCurrentMethod()!.Name}_{arg}") != 0)
        {
            Assert.Fail("This test should not have run a second time because the first run was successful.");
        }
    }

    [DesktopTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(0)]
    [InlineData(1)]
    [UISettings(MaxAttempts = 2)]
    public void FailsAllRetries(int arg)
    {
        _ = arg;
        Assert.Fail("Failure expected.");
    }
}
