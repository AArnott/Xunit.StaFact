// Copyright (c) Aaron Bockover. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System.Threading;

using Foundation;

namespace Xunit.Sdk
{
    internal sealed class CocoaSynchronizationContext : SynchronizationContext
    {

        private readonly Queue<KeyValuePair<SendOrPostCallback, object?>> messageQueue = new Queue<KeyValuePair<SendOrPostCallback, object?>>();
        private readonly int mainThread = Environment.CurrentManagedThreadId;
        private readonly AsyncAutoResetEvent workItemDone = new AsyncAutoResetEvent();
        private readonly string name;
        private readonly bool shouldSetAsCurrent;
        private int activeOperations;
        private bool pumping;
        private bool pumpingEnded;
        private ExceptionAggregator? aggregator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CocoaSynchronizationContext"/> class.
        /// </summary>
        public CocoaSynchronizationContext(string name, bool shouldSetAsCurrent)
        {
            this.name = name;
            this.shouldSetAsCurrent = shouldSetAsCurrent;
        }

        internal bool IsInContext => this.mainThread == Environment.CurrentManagedThreadId;

        private bool AnyMessagesInQueue
        {
            get
            {
                lock (this.messageQueue)
                {
                    return this.messageQueue.Count > 0;
                }
            }
        }

        private bool AnyPendingOperations => Volatile.Read(ref this.activeOperations) > 0;

        public override SynchronizationContext CreateCopy()
            => new CocoaSynchronizationContext(name, shouldSetAsCurrent);

        public override void Post(SendOrPostCallback d, object state)
            => NSRunLoop.Main.BeginInvokeOnMainThread(() => d(state));

        public override void Send(SendOrPostCallback d, object state)
            => NSRunLoop.Main.InvokeOnMainThread(() => d(state));

        /// <summary>
        /// Blocks the calling thread to pump messages until a task has completed.
        /// </summary>
        /// <param name="untilCompleted">The task that must complete to break out of the message loop.</param>
        public void PumpMessages(Task untilCompleted)
        {
            this.VerifyState();

            this.pumping = true;
            try
            {
                // Arrange to wake up immediately when the task completes.
                untilCompleted.ContinueWith(
                    _ =>
                    {
                        lock (this.messageQueue)
                        {
                            Monitor.Pulse(this.messageQueue);
                        }
                    },
                    TaskScheduler.Default);

                // Now run the message loop until the task completes.
                while (!untilCompleted.IsCompleted)
                {
                    this.TryOneWorkItem();
                }
            }
            finally
            {
                this.pumping = false;
                this.pumpingEnded = true;
            }
        }

        public async Task WaitForOperationCompletionAsync()
        {
            while (this.AnyPendingOperations || this.AnyMessagesInQueue)
            {
                await this.workItemDone.WaitAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Pump messages until all pending operations have completed
        /// and the message queue is empty.
        /// </summary>
        public void CompleteOperations()
        {
            this.VerifyState();
            this.pumping = true;
            try
            {
                while (this.AnyPendingOperations || this.AnyMessagesInQueue)
                {
                    this.TryOneWorkItem();
                }
            }
            finally
            {
                this.pumping = false;
            }
        }

        /// <inheritdoc />
        public override void OperationStarted()
        {
            Interlocked.Increment(ref this.activeOperations);
        }

        /// <inheritdoc />
        public override void OperationCompleted()
        {
            int result = Interlocked.Decrement(ref this.activeOperations);
            if (result == 0)
            {
                // Give any message waiter a heads up that the operation count has reached zero,
                // in case the queue is empty at the same time the operation count is, which
                // is usually a sign to return to its caller.
                lock (this.messageQueue)
                {
                    Monitor.Pulse(this.messageQueue);
                }
            }
        }

        internal void SetExceptionAggregator(ExceptionAggregator aggregator)
        {
            this.aggregator = aggregator;
        }

        private void VerifyState()
        {
            if (Environment.CurrentManagedThreadId != this.mainThread)
            {
                throw new InvalidOperationException("Wrong thread");
            }

            if (Current != this && this.shouldSetAsCurrent)
            {
                throw new InvalidOperationException("Wrong sync context");
            }

            if (this.pumping)
            {
                throw new InvalidOperationException("Already pumping");
            }
        }

        private bool TryOneWorkItem()
        {
            KeyValuePair<SendOrPostCallback, object?> work = default;
            lock (this.messageQueue)
            {
                if (this.messageQueue.Count == 0)
                {
                    Monitor.Wait(this.messageQueue);
                    return false;
                }

                work = this.messageQueue.Dequeue();
            }

            try
            {
                if (this.aggregator is object)
                {
                    this.aggregator.Run(() => work.Key(work.Value));
                }
                else
                {
                    work.Key(work.Value);
                }

                return true;
            }
            finally
            {
                this.workItemDone.Set();
            }
        }
    }
}
