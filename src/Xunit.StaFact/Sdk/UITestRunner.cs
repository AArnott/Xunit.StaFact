// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text;

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
                message = ReportFailure("Test", testFailed);
            }
            else if (message is ITestCleanupFailure testCleanupFailure)
            {
                message = ReportFailure("Test cleanup", testCleanupFailure);
            }

            return this.messageBus.QueueMessage(message);

            static IMessageSinkMessage ReportFailure<T>(string header, T message)
                where T : ITestMessage, IFailureInformation
            {
                // This test will run again; report it as skipped instead of failed.
                StringBuilder failures = new();
                failures.AppendLine($"{header} failed and will automatically retry. Failure details follow:");

                for (int i = 0; i < message.Messages.Length; i++)
                {
                    failures.AppendLine(message.Messages[i]);
                    failures.AppendLine(message.StackTraces[i]);
                }

                return new TestSkipped(message.Test, failures.ToString());
            }
        }
    }
}
