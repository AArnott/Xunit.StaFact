// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

internal static class UIUtilities
{
    private const UITestCase.SyncContextType ContextType = UITestCase.SyncContextType.Portable;
    private const string? SkipReason = null;

    internal static IXunitTestCase CreateTestCaseForFact(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        return Utilities.CreateTestCaseForFact(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            factAttribute);
    }

    internal static IXunitTestCase CreateTestCaseForDataRow(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute,
        ITheoryDataRow dataRow,
        object?[] testMethodArguments)
    {
        return Utilities.CreateTestCaseForDataRow(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            theoryAttribute,
            dataRow,
            testMethodArguments);
    }

    internal static IXunitTestCase CreateTestCaseForTheory(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute)
    {
        return Utilities.CreateTestCaseForTheory(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            theoryAttribute);
    }
}
