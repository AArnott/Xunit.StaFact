// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

public partial class UIFactTests
{
    [UIFact]
    public async Task UIFact_OnSTAThread()
    {
        int initialThread = Environment.CurrentManagedThreadId;
        SynchronizationContext? syncContext = SynchronizationContext.Current;
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        await Task.Yield();
        Assert.Equal(initialThread, Environment.CurrentManagedThreadId);
        Assert.Same(syncContext, SynchronizationContext.Current);
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
    }
}
