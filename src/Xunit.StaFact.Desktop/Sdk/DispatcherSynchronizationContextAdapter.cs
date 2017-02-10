// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    internal class DispatcherSynchronizationContextAdapter : SyncContextAdapter
    {
        internal static readonly SyncContextAdapter Default = new DispatcherSynchronizationContextAdapter();

        private DispatcherSynchronizationContextAdapter()
        {
        }

        internal override bool CanCompleteOperations => false;

        internal override SynchronizationContext Create() => new DispatcherSynchronizationContext();

        internal override void CompleteOperations()
        {
            throw new NotSupportedException("Async void test methods are not supported by the WPF dispatcher. Use Async Task instead.");
        }

        internal override void PumpTill(Task task)
        {
            if (!task.IsCompleted)
            {
                var frame = new DispatcherFrame();
                task.ContinueWith(_ => frame.Continue = false, TaskScheduler.Default);
                Dispatcher.PushFrame(frame);
            }
        }

        internal override void Run(Func<Task> work)
        {
            var task = work();
            this.PumpTill(task);
            task.GetAwaiter().GetResult();
        }

        internal override void Cleanup()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            base.Cleanup();
        }
    }
}
