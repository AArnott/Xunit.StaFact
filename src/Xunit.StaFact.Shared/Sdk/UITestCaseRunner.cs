// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

namespace Xunit.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Sdk;

    public class UITestCaseRunner : XunitTestCaseRunner
    {
        private SyncContextAdapter syncContextAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="UITestCaseRunner"/> class.
        /// </summary>
        /// <param name="testCase">The test case to be run.</param>
        /// <param name="displayName">The display name of the test case.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        /// <param name="syncContextAdapter">The <see cref="SynchronizationContext"/> adapter to use.</param>
        internal UITestCaseRunner(
            IXunitTestCase testCase,
            string displayName,
            string skipReason,
            object[] constructorArguments,
            object[] testMethodArguments,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            SyncContextAdapter syncContextAdapter)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
        {
            this.syncContextAdapter = syncContextAdapter;
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            return new UITestRunner(new XunitTest(this.TestCase, this.DisplayName), this.MessageBus, this.TestClass, this.ConstructorArguments, this.TestMethod, this.TestMethodArguments, this.SkipReason, this.BeforeAfterAttributes, this.Aggregator, this.CancellationTokenSource, this.syncContextAdapter).RunAsync();
        }
    }
}
