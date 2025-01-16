// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

internal class CocoaSynchronizationContextAdapter : SyncContextAdapter
{
    internal static readonly SyncContextAdapter Default = new CocoaSynchronizationContextAdapter();

    private CocoaSynchronizationContextAdapter()
    {
    }

    internal override SynchronizationContext Create(string name) => new CocoaSynchronizationContext(name, this.ShouldSetAsCurrent);

    internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
    {
        ((CocoaSynchronizationContext)synchronizationContext).PumpMessages(task);
    }
}
