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
        private readonly Thread ctorThread;

        public WpfFactTests()
        {
            this.ctorThread = Thread.CurrentThread;
        }

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

        [WpfFact]
        public void WpfFact_Void()
        {
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            Assert.IsType<DispatcherSynchronizationContext>(SynchronizationContext.Current);
            Assert.Same(this.ctorThread, Thread.CurrentThread);
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void AsyncVoidNotSupported()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
