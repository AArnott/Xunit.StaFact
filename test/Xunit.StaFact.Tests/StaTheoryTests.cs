// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using DesktopTheoryAttribute = Xunit.StaTheoryAttribute;

public partial class StaTheoryTests
{
    public static object[][] MemberDataSource => new object[][]
    {
        new object[] { 1, 2 },
        new object[] { 3, 4 },
    };

    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task StaTheory_OnSTAThread(int arg)
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();

        // Without a single-threaded SynchronizationContext, we won't come back to the STA thread.
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());

        Assert.True(arg == 0 || arg == 1);
    }

    [Trait("TestCategory", "FailureExpected")]
    [DesktopTheory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task StaTheoryFails(int arg)
    {
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();

        // Without a single-threaded SynchronizationContext, we won't come back to the STA thread.
        Assert.Null(SynchronizationContext.Current);
        Assert.Equal(ApartmentState.MTA, Thread.CurrentThread.GetApartmentState());

        Assert.False(arg == 0 || arg == 1);
    }

    [DesktopTheory]
    [MemberData(nameof(MemberDataSource))]
    public void MemberBasedTheory(int a, int b)
    {
        Assert.Equal(b, a + 1);
    }

    [StaTheory, Trait("TestCategory", "FailureExpected")]
    [InlineData(1)]
    public async Task OperationCanceledException_Thrown(int a)
    {
        Assert.Equal(1, a);
        await Task.Yield();
        throw new OperationCanceledException();
    }

    [DesktopTheory]
    [MemberData(nameof(NonSerializableObject.Data), MemberType = typeof(NonSerializableObject))]
    public void ThreadAffinitizedDataObject(NonSerializableObject o)
    {
        Assert.Equal(System.Diagnostics.Process.GetCurrentProcess().Id, o.ProcessId);
        Assert.Equal(Environment.CurrentManagedThreadId, o.ThreadId);
    }

    [StaTheory, Trait("TestCategory", "FailureExpected")]
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
