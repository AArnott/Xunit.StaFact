// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class NullAdapter : SyncContextAdapter
    {
#pragma warning disable SA1401
        internal static readonly SyncContextAdapter Default = new NullAdapter();
#pragma warning restore SA1401

        private NullAdapter()
        {
        }

        internal override bool CanCompleteOperations => false;

        internal override void CompleteOperations()
        {
            throw new NotSupportedException();
        }

        internal override SynchronizationContext Create() => null;

        internal override void PumpTill(Task task)
        {
            task.GetAwaiter().GetResult();
        }

        internal override void Run(Func<Task> work)
        {
            work().GetAwaiter().GetResult();
        }
    }
}
