// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

// This code came from the https://github.com/microsoft/vs-threading repo.
// Its copyright and license header is retained as follows:
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;

namespace Xunit;

internal static class TplExtensions
{
    /// <summary>
    /// Returns an awaitable for the specified task that will never throw, even if the source task
    /// faults or is canceled.
    /// </summary>
    /// <param name="task">The task whose completion should signal the completion of the returned awaitable.</param>
    /// <param name="captureContext">if set to <see langword="true" /> the continuation will be scheduled on the caller's context; <see langword="false" /> to always execute the continuation on the threadpool.</param>
    /// <returns>An awaitable.</returns>
    internal static NoThrowTaskAwaitable NoThrowAwaitable(this Task task, bool captureContext = true)
    {
        return new NoThrowTaskAwaitable(task, captureContext);
    }

    /// <summary>
    /// An awaitable that wraps a task and never throws an exception when waited on.
    /// </summary>
    internal readonly struct NoThrowTaskAwaitable
    {
        /// <summary>
        /// The task.
        /// </summary>
        private readonly Task task;

        /// <summary>
        /// A value indicating whether the continuation should be scheduled on the current sync context.
        /// </summary>
        private readonly bool captureContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoThrowTaskAwaitable" /> struct.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="captureContext">Whether the continuation should be scheduled on the current sync context.</param>
        internal NoThrowTaskAwaitable(Task task, bool captureContext)
        {
            this.task = task;
            this.captureContext = captureContext;
        }

        /// <summary>
        /// Gets the awaiter.
        /// </summary>
        /// <returns>The awaiter.</returns>
        public NoThrowTaskAwaiter GetAwaiter()
        {
            return new NoThrowTaskAwaiter(this.task, this.captureContext);
        }
    }

    /// <summary>
    /// An awaiter that wraps a task and never throws an exception when waited on.
    /// </summary>
    internal readonly struct NoThrowTaskAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// The task.
        /// </summary>
        private readonly Task task;

        /// <summary>
        /// A value indicating whether the continuation should be scheduled on the current sync context.
        /// </summary>
        private readonly bool captureContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoThrowTaskAwaiter"/> struct.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="captureContext">if set to <see langword="true" /> [capture context].</param>
        internal NoThrowTaskAwaiter(Task task, bool captureContext)
        {
            this.task = task;
            this.captureContext = captureContext;
        }

        /// <summary>
        /// Gets a value indicating whether the task has completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return this.task.IsCompleted; }
        }

        /// <summary>
        /// Schedules a delegate for execution at the conclusion of a task's execution.
        /// </summary>
        /// <param name="continuation">The action.</param>
        public void OnCompleted(Action continuation)
        {
            this.task.ConfigureAwait(this.captureContext).GetAwaiter().OnCompleted(continuation);
        }

        /// <summary>
        /// Schedules a delegate for execution at the conclusion of a task's execution
        /// without capturing the ExecutionContext.
        /// </summary>
        /// <param name="continuation">The action.</param>
        public void UnsafeOnCompleted(Action continuation)
        {
            this.task.ConfigureAwait(this.captureContext).GetAwaiter().UnsafeOnCompleted(continuation);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void GetResult()
        {
            // Never throw here.
        }
    }
}
