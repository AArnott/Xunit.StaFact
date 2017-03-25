// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A base class that abstracts away particulars of a specific
    /// <see cref="SynchronizationContext"/> derived type.
    /// </summary>
    internal abstract class SyncContextAdapter
    {
        /// <summary>
        /// Gets a value indicating whether async void methods are supported.
        /// </summary>
        /// <value><c>true</c> if <see cref="CompleteOperations()"/> can be invoked.</value>
        internal virtual bool CanCompleteOperations => true;

        /// <summary>
        /// Creates a new <see cref="SynchronizationContext"/> of the derived type.
        /// </summary>
        /// <returns>The new instance.</returns>
        internal abstract SynchronizationContext Create();

        /// <summary>
        /// Executes an async delegate while synchronously blocking the calling thread,
        /// but without deadlocking.
        /// </summary>
        /// <param name="work">The async delegate.</param>
        internal abstract void Run(Func<Task> work);

        /// <summary>
        /// Pumps messages until a task completes.
        /// </summary>
        /// <param name="task">The task to wait on.</param>
        internal abstract void PumpTill(Task task);

        /// <summary>
        /// Pump messages until all pending operations have completed
        /// and the message queue is empty.
        /// </summary>
        internal abstract void CompleteOperations();

        /// <summary>
        /// Clean up this instance.
        /// </summary>
        internal virtual void Cleanup()
        {
        }
    }
}
