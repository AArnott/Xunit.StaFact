// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class WinFormsSynchronizationContextAdapter : SyncContextAdapter
    {
        internal static readonly SyncContextAdapter Default = new WinFormsSynchronizationContextAdapter();

        private WinFormsSynchronizationContextAdapter()
        {
        }

        internal override bool CanCompleteOperations => false;

        internal override SynchronizationContext Create() => new WindowsFormsSynchronizationContext();

        internal override void CompleteOperations()
        {
            throw new NotSupportedException("Async void test methods are not supported by the WinForms dispatcher. Use Async Task instead.");
        }

        internal override void PumpTill(Task task)
        {
            while (!task.IsCompleted)
            {
                Application.DoEvents();
                Thread.Sleep(0); // give up thread to OS so we don't spin CPU too hard
            }
        }

        internal override void InitializeThread() => Application.OleRequired();

        internal override void Run(Func<Task> work)
        {
            var task = work();
            this.PumpTill(task);
            task.GetAwaiter().GetResult();
        }
    }
}
