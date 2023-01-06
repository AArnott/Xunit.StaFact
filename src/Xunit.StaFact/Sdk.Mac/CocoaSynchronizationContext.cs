// Copyright (c) Aaron Bockover. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

using System.Threading;

using Foundation;

namespace Xunit.Sdk
{
    internal sealed class CocoaSynchronizationContext : SynchronizationContext
    {
        public override SynchronizationContext CreateCopy()
            => new CocoaSynchronizationContext();

        public override void Post(SendOrPostCallback d, object state)
            => NSRunLoop.Main.BeginInvokeOnMainThread(() => d(state));

        public override void Send(SendOrPostCallback d, object state)
            => NSRunLoop.Main.InvokeOnMainThread(() => d(state));
    }
}