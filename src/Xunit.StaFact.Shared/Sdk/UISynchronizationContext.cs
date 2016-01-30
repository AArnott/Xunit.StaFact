// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal class UISynchronizationContext : SynchronizationContext
    {
        private readonly Queue<KeyValuePair<SendOrPostCallback, object>> messageQueue = new Queue<KeyValuePair<SendOrPostCallback, object>>();
        private readonly int mainThread = Environment.CurrentManagedThreadId;
        private int activeOperations;
        private bool pumping;

        /// <summary>
        /// Initializes a new instance of the <see cref="UISynchronizationContext"/> class.
        /// </summary>
        public UISynchronizationContext()
        {
        }

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

        /// <summary>
        /// Executes an async delegate while synchronously blocking the calling thread,
        /// but without deadlocking.
        /// </summary>
        /// <param name="work">The async delegate.</param>
        public void Run(Func<Task> work)
        {
            this.VerifyState();
            Task task = work();
            this.PumpMessages(task);
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
            if (Environment.CurrentManagedThreadId == this.mainThread)
            {
                d(state);
            }
            else
            {
                Exception ex = null;
                var evt = new ManualResetEventSlim();
                this.Post(
                    _ =>
                    {
                        try
                        {
                            d(state);
                        }
                        catch (Exception e)
                        {
                            ex = e;
                        }
                        finally
                        {
                            evt.Set();
                        }
                    },
                    null);
                evt.Wait();
                if (ex != null)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
            }
        }

        private void VerifyState()
        {
            if (Environment.CurrentManagedThreadId != this.mainThread)
            {
                throw new InvalidOperationException("Wrong thread");
            }

            if (SynchronizationContext.Current != this)
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
            KeyValuePair<SendOrPostCallback, object> work = default(KeyValuePair<SendOrPostCallback, object>);
            lock (this.messageQueue)
            {
                if (this.messageQueue.Count == 0)
                {
                    Monitor.Wait(this.messageQueue);
                    return false;
                }

                work = this.messageQueue.Dequeue();
            }

            work.Key(work.Value);
            return true;
        }

        internal class Adapter : SyncContextAdapter
        {
#pragma warning disable SA1401
            internal static readonly Adapter Default = new Adapter();
#pragma warning restore SA1401

            private Adapter()
            {
            }

            private static UISynchronizationContext UISyncContext => (UISynchronizationContext)SynchronizationContext.Current;

            internal override SynchronizationContext Create() => new UISynchronizationContext();

            internal override void CompleteOperations()
            {
                UISyncContext.CompleteOperations();
            }

            internal override void PumpTill(Task task)
            {
                UISyncContext.PumpMessages(task);
            }

            internal override void Run(Func<Task> work)
            {
                UISyncContext.Run(work);
            }
        }
    }
}
