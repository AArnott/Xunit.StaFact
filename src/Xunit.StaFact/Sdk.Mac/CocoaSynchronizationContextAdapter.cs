// Copyright (c) Aaron Bockover. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Foundation;

namespace Xunit.Sdk
{
    internal class CocoaSynchronizationContextAdapter : SyncContextAdapter
    {
        internal static readonly SyncContextAdapter Default = new CocoaSynchronizationContextAdapter();

        private CocoaSynchronizationContextAdapter()
        {
        }

        internal override bool CanCompleteOperations => true;

        internal override SynchronizationContext Create(string name) => new CocoaSynchronizationContext();

        internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext)
        {
            throw new NotSupportedException("Async void test methods are not supported by the WinForms dispatcher. Use Async Task instead.");
        }

        // internal override void CompleteOperations()
        // {
        // }

        internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
        {
            ((CocoaSynchronizationContext)syncContext).PumpMessages(task);
        }
    }
}
