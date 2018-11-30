// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE.txt file in the project root for full license information.

#if !NET45

namespace Xunit.Sdk
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;

    public class WinFormsTheoryTestCase : XunitTheoryTestCase
    {
        public WinFormsTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod)
        {
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            => new WinFormsTheoryTestCaseRunner(this, this.DisplayName, this.SkipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource).RunAsync();
    }
}

#endif