// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

internal class AppKitSynchronizationContextAdapter : SyncContextAdapter
{
    internal static readonly SyncContextAdapter Default = new AppKitSynchronizationContextAdapter();

    private AppKitSynchronizationContextAdapter()
    {
    }

    internal override bool CanCompleteOperations => true;

    internal override SynchronizationContext Create(string name) => new AppKitSynchronizationContext(name, this.ShouldSetAsCurrent);

    internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext) => ((AppKitSynchronizationContext)syncContext).WaitForOperationCompletionAsync();

    internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
    {
        ((AppKitSynchronizationContext)synchronizationContext).PumpMessages(task);
    }
}
