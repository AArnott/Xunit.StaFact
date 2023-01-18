// Copyright (c) Aaron Bockover. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Foundation;

namespace Xunit.Sdk;

internal class CocoaSynchronizationContextAdapter : SyncContextAdapter
{
    internal static readonly SyncContextAdapter Default = new CocoaSynchronizationContextAdapter();

    private CocoaSynchronizationContextAdapter()
    {
    }

    internal override bool CanCompleteOperations => true;

    internal override SynchronizationContext Create(string name) => new CocoaSynchronizationContext(name, this.ShouldSetAsCurrent);

    internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext) => ((CocoaSynchronizationContext)syncContext).WaitForOperationCompletionAsync();

    internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
    {
        ((CocoaSynchronizationContext)synchronizationContext).PumpMessages(task);
    }
}
