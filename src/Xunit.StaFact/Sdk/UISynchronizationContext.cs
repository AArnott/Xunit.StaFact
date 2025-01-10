// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.ExceptionServices;

namespace Xunit.Sdk;

/// <summary>
/// A portable implementation of a single-threaded SynchronizationContext.
/// </summary>
internal class UISynchronizationContext : SynchronizationContext
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
    /// Initializes a new instance of the <see cref="UISynchronizationContext"/> class.
    /// </summary>
    public UISynchronizationContext(string name, bool shouldSetAsCurrent)
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
    public override void Post(SendOrPostCallback d, object? state)
    {
        if (this.pumpingEnded)
        {
            throw new InvalidOperationException($"The message pump in '{this.name}' isn't running any more.");
        }

        lock (this.messageQueue)
        {
            this.messageQueue.Enqueue(new KeyValuePair<SendOrPostCallback, object?>(d, state));
            Monitor.Pulse(this.messageQueue);
        }
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        if (Environment.CurrentManagedThreadId == this.mainThread)
        {
            d(state);
        }
        else
        {
            Exception? ex = null;
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
            if (this.aggregator.HasValue)
            {
                this.aggregator.Value.Run(() => work.Key(work.Value));
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

    internal class Adapter : SyncContextAdapter
    {
        internal static readonly Adapter Default = new Adapter();

        protected Adapter()
        {
        }

        internal override SynchronizationContext Create(string name) => new UISynchronizationContext(name, this.ShouldSetAsCurrent);

        internal override Task WaitForOperationCompletionAsync(SynchronizationContext syncContext) => ((UISynchronizationContext)syncContext).WaitForOperationCompletionAsync();

        internal override void PumpTill(SynchronizationContext syncContext, Task task)
        {
            ((UISynchronizationContext)syncContext).PumpMessages(task);
        }
    }
}
