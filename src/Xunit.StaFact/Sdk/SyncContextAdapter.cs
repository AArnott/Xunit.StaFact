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
        /// <value><c>true</c> if <see cref="WaitForOperationCompletionAsync(SynchronizationContext)"/> can be invoked.</value>
        internal virtual bool CanCompleteOperations => true;

        internal virtual bool ShouldSetAsCurrent => true;

        /// <summary>
        /// Creates a new <see cref="SynchronizationContext"/> of the derived type.
        /// </summary>
        /// <returns>The new instance.</returns>
        internal abstract SynchronizationContext Create();

        /// <summary>
        /// Runs code on the test thread before any user code is executed (before the test class is even instantiated).
        /// </summary>
        internal virtual void InitializeThread()
        {
        }

        /// <summary>
        /// Pumps messages until a task completes.
        /// </summary>
        /// <param name="syncContext">The <see cref="SynchronizationContext"/> returned from <see cref="Create"/>.</param>
        /// <param name="task">The task to wait on.</param>
        internal abstract void PumpTill(SynchronizationContext syncContext, Task task);

        /// <summary>
        /// Returns a <see cref="Task"/> that completes when all pending operations have completed.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when all pending operations have completed.</returns>
        internal abstract Task WaitForOperationCompletionAsync(SynchronizationContext syncContext);

        /// <summary>
        /// Clean up this instance.
        /// </summary>
        internal virtual void Cleanup()
        {
        }
    }
}
