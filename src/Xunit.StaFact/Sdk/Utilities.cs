// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Xunit.Sdk
{
    internal static class Utilities
    {
        internal static SyncContextAwaiter GetAwaiter(this SynchronizationContext synchronizationContext) => new SyncContextAwaiter(synchronizationContext);

        internal struct SyncContextAwaiter : ICriticalNotifyCompletion
        {
            private readonly SynchronizationContext synchronizationContext;

            internal SyncContextAwaiter(SynchronizationContext synchronizationContext)
            {
                this.synchronizationContext = synchronizationContext;
            }

            /// <summary>
            /// Gets a value indicating whether the caller is already running on the desired <see cref="SynchronizationContext"/>.
            /// </summary>
            public bool IsCompleted => (this.synchronizationContext is UISynchronizationContext uiSyncContext && uiSyncContext.IsInContext) || (SynchronizationContext.Current == this.synchronizationContext);

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                this.synchronizationContext.Post(s => ((Action)s!)(), continuation);
            }

            public void UnsafeOnCompleted(Action continuation) => this.OnCompleted(continuation);
        }
    }
}
