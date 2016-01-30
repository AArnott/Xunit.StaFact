// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.StaFact.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    public class WpfFactTests
    {
        [WpfFact]
        public async Task WpfFact_OnSTAThread()
        {
            Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            var syncContext = SynchronizationContext.Current;
            await Task.Yield();
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
            Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
        }

        ////[WpfTheory(Skip = "Fails at command line")]
        [InlineData(0)]
        public async Task WpfTheory_OnSTAThread(int unused)
        {
            Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            await Task.Yield();
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState()); // still there
            Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
        }

        [WpfFact, Trait("Category", "FailureExpected")]
        public async void AsyncVoidNotSupported()
        {
        }

        [WpfFact, Trait("Category", "FailureExpected")]
        public async Task FailAfterYield_Task()
        {
            await Task.Yield();
            Assert.False(true);
        }

        [WpfFact, Trait("Category", "FailureExpected")]
        public async Task FailAfterDelay_Task()
        {
            await Task.Delay(10);
            Assert.False(true);
        }
    }
}
