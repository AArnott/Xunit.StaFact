// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

public class UITestCaseRunner : XunitTestCaseRunner
{
    private UISettingsAttribute settings;
    private ThreadRental threadRental;

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

    protected override async ValueTask<RunSummary> RunTest(XunitTestCaseRunnerContext ctxt, IXunitTest test)
    {
        RunSummary result = default;
        for (int i = 0; i < this.settings.MaxAttempts; i++)
        {
            bool finalAttempt = i == this.settings.MaxAttempts - 1;
            RunSummary summary = await new UITestRunner(new XunitTest(this.TestCase, this.DisplayName), this.MessageBus, finalAttempt, this.TestClass, this.ConstructorArguments, this.TestMethod, this.TestMethodArguments, this.SkipReason, this.BeforeAfterAttributes, this.Aggregator, this.CancellationTokenSource, this.threadRental).RunAsync();
            result.Aggregate(summary);
            if (summary.Failed == 0)
            {
                break;
            }
        }

        return result;
    }
}
