// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Xunit.Sdk;

public class UITestCaseRunner : XunitTestCaseRunnerBase<UITestCaseRunnerContext, IXunitTestCase, IXunitTest>
{
    private readonly UISettingsAttribute settings;
    private readonly ThreadRental threadRental;

    /// <summary>
    /// Initializes a new instance of the <see cref="UITestCaseRunner"/> class.
    /// </summary>
    /// <param name="settings">The settings to use for this test case.</param>
    /// <param name="threadRental">The <see cref="ThreadRental"/> instance to use.</param>
    internal UITestCaseRunner(
        UISettingsAttribute settings,
        ThreadRental threadRental)
    {
        this.settings = settings;
        this.threadRental = threadRental;
    }

    public async ValueTask<RunSummary> Run(
       IXunitTestCase testCase,
       IMessageBus messageBus,
       ExceptionAggregator aggregator,
       CancellationTokenSource cancellationTokenSource,
       string displayName,
       string? skipReason,
       ExplicitOption explicitOption,
       object?[] constructorArguments)
    {
        if (testCase is null)
        {
            throw new ArgumentNullException(nameof(testCase));
        }

        // This code comes from XunitRunnerHelper.RunXunitTestCase,
        // and it's centralized here just so we don't have to duplicate
        // it in both `UITestCase` and `UITheoryTestCase`.
        IReadOnlyCollection<IXunitTest> tests = await aggregator.RunAsync(testCase.CreateTests, []);

        if (aggregator.ToException() is Exception ex)
        {
            if (ex.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal))
            {
                return XunitRunnerHelper.SkipTestCases(
                    messageBus,
                    cancellationTokenSource,
                    [testCase],
                    ex.Message.Substring(DynamicSkipToken.Value.Length),
                    sendTestCaseMessages: false);
            }
            else
            {
                return XunitRunnerHelper.FailTestCases(
                    messageBus,
                    cancellationTokenSource,
                    [testCase],
                    ex,
                    sendTestCaseMessages: false);
            }
        }

        await using UITestCaseRunnerContext ctxt = new(
            this.settings,
            this.threadRental,
            testCase,
            tests,
            messageBus,
            aggregator,
            cancellationTokenSource,
            displayName,
            skipReason,
            explicitOption,
            constructorArguments);

        await ctxt.InitializeAsync();

        return await this.Run(ctxt);
    }

    internal static ValueTask<RunSummary> Run(
        IXunitTestCase testCase,
        SyncContextAdapter adapter,
        UISettingsAttribute settings,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        var task = Task.Run(
            async () =>
            {
                using ThreadRental threadRental = await ThreadRental.CreateAsync(adapter, testCase.TestMethod);
                await threadRental.SynchronizationContext;
                var runner = new UITestCaseRunner(settings, threadRental);
                return await runner.Run(
                    testCase,
                    messageBus,
                    aggregator,
                    cancellationTokenSource,
                    testCase.TestCaseDisplayName,
                    testCase.SkipReason,
                    explicitOption,
                    constructorArguments);
            },
            cancellationTokenSource.Token);

        // We need to block the XUnit thread to ensure its concurrency throttle is effective.
        // See https://github.com/AArnott/Xunit.StaFact/pull/55#issuecomment-826187354 for details.
        return new(task.GetAwaiter().GetResult());
    }

    protected async override ValueTask<RunSummary> RunTest(UITestCaseRunnerContext ctxt, IXunitTest test)
    {
        if (ctxt is null)
        {
            throw new ArgumentNullException(nameof(ctxt));
        }

        RunSummary result = default;
        for (int i = 0; i < this.settings.MaxAttempts; i++)
        {
            bool finalAttempt = i == this.settings.MaxAttempts - 1;
            RunSummary summary = await UITestRunner.Instance.Run(
                ctxt.Settings,
                ctxt.ThreadRental,
                test,
                finalAttempt ? ctxt.MessageBus : new FilteringMessageBus(ctxt.MessageBus),
                ctxt.ConstructorArguments,
                ctxt.ExplicitOption,
                ctxt.Aggregator.Clone(),
                ctxt.CancellationTokenSource,
                ctxt.BeforeAfterTestAttributes);
            result.Aggregate(summary);
            if (summary.Failed == 0)
            {
                break;
            }
        }

        return result;
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
                where T : ITestMessage, IErrorMetadata
            {
                // This test will run again; report it as skipped instead of failed.
                StringBuilder failures = new();
                failures.AppendLine($"{header} failed and will automatically retry. Failure details follow:");

                for (int i = 0; i < message.Messages.Length; i++)
                {
                    failures.AppendLine(message.Messages[i]);
                    failures.AppendLine(message.StackTraces[i]);
                }

                IExecutionMetadata? executionMetadata = message as IExecutionMetadata;
                ITestResultMessage? testResultMessage = message as ITestResultMessage;
                return new TestSkipped
                {
                    Reason = failures.ToString(),
                    AssemblyUniqueID = message.AssemblyUniqueID,
                    TestCollectionUniqueID = message.TestCollectionUniqueID,
                    TestClassUniqueID = message.TestClassUniqueID,
                    TestMethodUniqueID = message.TestMethodUniqueID,
                    TestUniqueID = message.TestUniqueID,
                    TestCaseUniqueID = message.TestCaseUniqueID,
                    Output = executionMetadata?.Output ?? string.Empty,
                    Warnings = executionMetadata?.Warnings ?? [],
                    ExecutionTime = executionMetadata?.ExecutionTime ?? 0,
                    FinishTime = testResultMessage?.FinishTime ?? DateTimeOffset.Now,
                };
            }
        }
    }
}
