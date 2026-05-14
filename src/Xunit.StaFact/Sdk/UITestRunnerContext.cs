// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

public class UITestRunnerContext : XunitTestRunnerBaseContext<IXunitTest>
{
    internal UITestRunnerContext(
        UISettingsAttribute settings,
        ThreadRental threadRental,
        IXunitTest test,
        IMessageBus messageBus,
        ExplicitOption explicitOption,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
        object?[] constructorArguments)
        : base(test, messageBus, explicitOption, aggregator, cancellationTokenSource, beforeAfterTestAttributes, constructorArguments)
    {
        this.Settings = settings;
        this.ThreadRental = threadRental;
    }

    public UISettingsAttribute Settings { get; }

    internal ThreadRental ThreadRental { get; }
}
