// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#nullable enable

namespace Xunit.Sdk
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// An asynchronous implementation of an AutoResetEvent.
    /// </summary>
    [DebuggerDisplay("Signaled: {signaled}")]
    internal class AsyncAutoResetEvent
    {
        /// <summary>
        /// A queue of folks awaiting signals.
        /// </summary>
        private readonly Queue<TaskCompletionSource<object?>> signalAwaiters = new Queue<TaskCompletionSource<object?>>();

        /// <summary>
        /// A value indicating whether this event is already in a signaled state.
        /// </summary>
        /// <devremarks>
        /// This should not need the volatile modifier because it is
        /// always accessed within a lock.
        /// </devremarks>
        private bool signaled;

        /// <summary>
        /// Returns an awaitable that may be used to asynchronously acquire the next signal.
        /// </summary>
        /// <returns>An awaitable.</returns>
        public Task WaitAsync()
        {
            lock (this.signalAwaiters)
            {
                if (this.signaled)
                {
                    this.signaled = false;
                    return Task.CompletedTask;
                }
                else
                {
                    var waiter = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                    this.signalAwaiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        /// <summary>
        /// Sets the signal if it has not already been set, allowing one awaiter to handle the signal if one is already waiting.
        /// </summary>
        public void Set()
        {
            TaskCompletionSource<object?>? toRelease = null;
            lock (this.signalAwaiters)
            {
                if (this.signalAwaiters.Count > 0)
                {
                    toRelease = this.signalAwaiters.Dequeue();
                }
                else if (!this.signaled)
                {
                    this.signaled = true;
                }
            }

            toRelease?.TrySetResult(null);
        }
    }
}
