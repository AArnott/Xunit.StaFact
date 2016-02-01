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
    using System.Windows.Forms;

    public class WinFormsFactTests
    {
        private readonly Thread ctorThread;

        public WinFormsFactTests()
        {
            this.ctorThread = Thread.CurrentThread;
        }

        [WinFormsFact]
        public void Void()
        {
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
            Assert.Same(this.ctorThread, Thread.CurrentThread);
        }

        [WinFormsFact]
        public async Task AsyncTask()
        {
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
            Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
            Assert.Same(this.ctorThread, Thread.CurrentThread);

            await Task.Yield();

            Assert.Same(this.ctorThread, Thread.CurrentThread);
            Assert.IsType<WindowsFormsSynchronizationContext>(SynchronizationContext.Current);
        }

        [WinFormsFact, Trait("Category", "FailureExpected")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void AsyncVoidNotSupported()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }

        [WinFormsFact, Trait("Category", "FailureExpected")]
        public async Task FailAfterYield_Task()
        {
            await Task.Yield();
            Assert.False(true);
        }

        [WinFormsFact, Trait("Category", "FailureExpected")]
        public async Task FailAfterDelay_Task()
        {
            await Task.Delay(10);
            Assert.False(true);
        }
    }
}
