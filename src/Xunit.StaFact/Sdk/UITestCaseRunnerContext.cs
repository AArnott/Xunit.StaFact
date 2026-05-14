// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

public class UITestCaseRunnerContext : XunitTestCaseRunnerBaseContext<IXunitTestCase, IXunitTest>
{
    internal UITestCaseRunnerContext(
        UISettingsAttribute settings,
        ThreadRental threadRental,
        IXunitTestCase testCase,
        IReadOnlyCollection<IXunitTest> tests,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        string displayName,
        string? skipReason,
        ExplicitOption explicitOption,
        object?[] constructorArguments)
        : base(testCase, tests, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, explicitOption, constructorArguments)
    {
        this.Settings = settings;
        this.ThreadRental = threadRental;
    }

    public UISettingsAttribute Settings { get; }

    internal ThreadRental ThreadRental { get; }
}
