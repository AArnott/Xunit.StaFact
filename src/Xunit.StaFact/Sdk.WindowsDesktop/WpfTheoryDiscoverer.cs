// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WpfTheoryAttribute"/>.
/// </summary>
public class WpfTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        IXunitTestCase testCase = WpfUtilities.CreateTestCaseForDataRow(
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
        IXunitTestCase testCase = WpfUtilities.CreateTestCaseForTheory(
            discoveryOptions,
            testMethod,
            theoryAttribute);
        return new([testCase]);
    }
}
