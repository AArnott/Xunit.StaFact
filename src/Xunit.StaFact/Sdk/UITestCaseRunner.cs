// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

public class UITestCaseRunner : XunitTestCaseRunner
{
    private UISettingsKey settings;
    private ThreadRental threadRental;

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
    /// <param name="settings">The settings to use for this test case.</param>
    /// <param name="threadRental">The <see cref="ThreadRental"/> instance to use.</param>
    internal UITestCaseRunner(
        IXunitTestCase testCase,
        string displayName,
        string skipReason,
        object[] constructorArguments,
        object[] testMethodArguments,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        UISettingsKey settings,
        ThreadRental threadRental)
        : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
    {
        this.settings = settings;
        this.threadRental = threadRental;
    }

    protected override async Task<RunSummary> RunTestAsync()
    {
        var result = new RunSummary();
        for (int i = 0; i < this.settings.MaxAttempts; i++)
        {
            bool finalAttempt = i == this.settings.MaxAttempts - 1;
            RunSummary summary = await new UITestRunner(new XunitTest(this.TestCase, this.DisplayName), this.MessageBus, finalAttempt, this.TestClass, this.ConstructorArguments, this.TestMethod, this.TestMethodArguments, this.SkipReason, this.BeforeAfterAttributes, this.Aggregator, this.CancellationTokenSource, this.threadRental).RunAsync();
            result.Aggregate(summary);
            if (summary.Skipped == 0)
            {
                // Failed tests prior to the final attempt are reported as skipped. If the current execution did not
                // contain any skipped results, we know the summary is valid for the test as a whole.
                break;
            }
        }

        return result;
    }
}
