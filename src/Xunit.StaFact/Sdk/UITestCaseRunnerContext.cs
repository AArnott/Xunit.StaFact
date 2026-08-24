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
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        ExceptionAggregator aggregator,
        string displayName,
        string? skipReason,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        object?[] constructorArguments,
        FixtureMappingManager methodFixtureMappings)
        : base(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, parallelMode, scheduler, constructorArguments, methodFixtureMappings)
    {
        this.Settings = settings;
        this.ThreadRental = threadRental;
    }

    public UISettingsAttribute Settings { get; }

    internal ThreadRental ThreadRental { get; }
}
