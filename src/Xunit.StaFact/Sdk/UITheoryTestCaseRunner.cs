// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Xunit.Sdk;

public class UITheoryTestCaseRunner : XunitTheoryTestCaseRunner
{
    private readonly UISettingsKey settings;
    private readonly ThreadRental threadRental;

    internal UITheoryTestCaseRunner(UITheoryTestCase testCase, string displayName, string skipReason, object[] constructorArguments, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, UISettingsKey settings, ThreadRental threadRental)
        : base(testCase, displayName, skipReason, constructorArguments, diagnosticMessageSink, messageBus, aggregator, cancellationTokenSource)
    {
        this.settings = settings;
        this.threadRental = threadRental;
    }

    internal new UITheoryTestCase TestCase => (UITheoryTestCase)base.TestCase;

    protected override XunitTestRunner CreateTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
    {
        if (this.settings.MaxAttempts != 1)
        {
            throw new NotSupportedException("MaxAttempts can only be set for pre-enumerated theories.");
        }

        return new UITestRunner(test, messageBus, finalAttempt: true, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource, this.threadRental);
    }
}
