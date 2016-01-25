// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class SyncContextAdapter
    {
        internal abstract SynchronizationContext Create();

        internal abstract void Run(Func<Task> work);

        internal abstract void PumpTill(Task task);

        internal abstract void CompleteOperations();
    }
}
