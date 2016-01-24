// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class UISynchronizationContext : SynchronizationContext
    {
        private readonly Queue<KeyValuePair<SendOrPostCallback, object>> messageQueue = new Queue<KeyValuePair<SendOrPostCallback, object>>();
        private readonly int mainThread = Environment.CurrentManagedThreadId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UISynchronizationContext"/> class.
        /// </summary>
        public UISynchronizationContext()
        {
        }

        /// <summary>
        /// Blocks the calling thread to pump messages until a task has completed.
        /// </summary>
        /// <param name="untilCompleted">The task that must complete to break out of the message loop.</param>
        public void PumpMessages(Task untilCompleted)
        {
            if (Environment.CurrentManagedThreadId != this.mainThread)
            {
                throw new InvalidOperationException("Wrong thread");
            }

            untilCompleted.ContinueWith(_ => Monitor.Pulse(this.messageQueue));
            while (!untilCompleted.IsCompleted)
            {
                KeyValuePair<SendOrPostCallback, object> work = default(KeyValuePair<SendOrPostCallback, object>);
                lock (this.messageQueue)
                {
                    if (this.messageQueue.Count == 0)
                    {
                        Monitor.Wait(this.messageQueue);
                        continue;
                    }

                    work = this.messageQueue.Dequeue();
                }

                work.Key(work.Value);
            }
        }

        /// <inheritdoc />
        public override void Post(SendOrPostCallback d, object state)
        {
            lock (this.messageQueue)
            {
                this.messageQueue.Enqueue(new KeyValuePair<SendOrPostCallback, object>(d, state));
                Monitor.Pulse(this.messageQueue);
            }
        }

        /// <inheritdoc />
        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException();
        }
    }
}
