// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Windows.Threading;

namespace Xunit.Sdk
{
    internal class DispatcherSynchronizationContextAdapter : SyncContextAdapter
    {
        internal static readonly SyncContextAdapter Default = new DispatcherSynchronizationContextAdapter();

        private DispatcherSynchronizationContextAdapter()
        {
        }

        internal override bool CanCompleteOperations => false;

        internal override SynchronizationContext Create(string name) => new DispatcherSynchronizationContext();

        internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext)
        {
            throw new NotSupportedException("Async void test methods are not supported by the WPF dispatcher. Use Async Task instead.");
        }

        internal override void PumpTill(SynchronizationContext synchronizationContext, Task task)
        {
            if (!task.IsCompleted)
            {
                var frame = new DispatcherFrame();
                task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
                Dispatcher.PushFrame(frame);
            }
        }

        internal override void Cleanup()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            base.Cleanup();
        }
    }
}
