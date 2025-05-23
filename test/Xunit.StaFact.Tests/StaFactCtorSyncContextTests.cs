﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

public class StaFactCtorSyncContextTests : IDisposable
{
    private readonly SynchronizationContext ctorSyncContext;

    public StaFactCtorSyncContextTests()
    {
        this.ctorSyncContext = new SynchronizationContext();
        Assert.Null(SynchronizationContext.Current);
        SynchronizationContext.SetSynchronizationContext(this.ctorSyncContext);
    }

    [StaFact]
    public void SyncContextPreservedFromCtor()
    {
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }

    public void Dispose()
    {
        Assert.Same(this.ctorSyncContext, SynchronizationContext.Current);
    }
}
