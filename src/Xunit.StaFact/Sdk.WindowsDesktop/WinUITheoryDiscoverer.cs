// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

#if WINDOWS10_0_17763_0_OR_GREATER

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinUITheoryAttribute"/>.
/// </summary>
public class WinUITheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments, string? index)
    {
        IXunitTestCase testCase = WinUIUtilities.CreateTestCaseForDataRow(
            discoveryOptions,
            testMethod,
            theoryAttribute,
            dataRow,
            testMethodArguments);
        return new([testCase]);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        IXunitTestCase testCase = WinUIUtilities.CreateTestCaseForTheory(
            discoveryOptions,
            testMethod,
            theoryAttribute);
        return new([testCase]);
    }
}

#endif
