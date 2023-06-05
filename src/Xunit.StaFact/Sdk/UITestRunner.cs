// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Xunit.Sdk;

public class UITestRunner : XunitTestRunner
{
    private readonly ThreadRental threadRental;

    internal UITestRunner(ITest test, IMessageBus messageBus, bool finalAttempt, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, ThreadRental threadRental)
        : base(test, finalAttempt ? messageBus : new FilteringMessageBus(messageBus), testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
    {
        this.threadRental = threadRental;
    }

    protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
    {
        return new UITestInvoker(this.Test, this.MessageBus, this.TestClass, this.ConstructorArguments, this.TestMethod, this.TestMethodArguments, this.BeforeAfterAttributes, aggregator, this.CancellationTokenSource, this.threadRental).RunAsync();
    }

    private sealed class FilteringMessageBus : IMessageBus
    {
        private readonly IMessageBus messageBus;

        public FilteringMessageBus(IMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public void Dispose()
        {
            this.messageBus.Dispose();
        }

        public bool QueueMessage(IMessageSinkMessage message)
        {
            if (message is ITestFailed testFailed)
            {
                // This test will run again; report it as skipped instead of failed
                // TODO: What kind of additional logs should we include?
                message = new TestSkipped(testFailed.Test, "Test will automatically retry.");
            }
            else if (message is ITestCleanupFailure testCleanupFailure)
            {
                // This test will run again; report it as skipped instead of failed
                // TODO: What kind of additional logs should we include?
                message = new TestSkipped(testCleanupFailure.Test, "Test will automatically retry.");
            }

            return this.messageBus.QueueMessage(message);
        }
    }
}
