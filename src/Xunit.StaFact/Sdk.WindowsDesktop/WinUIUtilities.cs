// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

internal static class WinUIUtilities
{
    private const UITestCase.SyncContextType ContextType = UITestCase.SyncContextType.WinUI;
    private static readonly string? SkipReason = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : "WinUI only exists on Windows.";

    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForFact(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        return Utilities.CreateTestCasesForFact(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            factAttribute);
    }

    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForDataRow(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute,
        ITheoryDataRow dataRow,
        object?[] testMethodArguments)
    {
        return Utilities.CreateTestCasesForDataRow(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            theoryAttribute,
            dataRow,
            testMethodArguments);
    }

    internal static IReadOnlyCollection<IXunitTestCase> CreateTestCasesForTheory(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute)
    {
        return Utilities.CreateTestCasesForTheory(
            ContextType,
            SkipReason,
            discoveryOptions,
            testMethod,
            theoryAttribute);
    }
}

#endif
