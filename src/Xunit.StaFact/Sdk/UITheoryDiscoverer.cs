// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="UITheoryAttribute"/>.
/// </summary>
public class UITheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments, string? index)
    {
        IReadOnlyCollection<IXunitTestCase> testCases = UIUtilities.CreateTestCasesForDataRow(
            discoveryOptions,
            testMethod,
            theoryAttribute,
            dataRow,
            testMethodArguments);
        return new(testCases);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        IReadOnlyCollection<IXunitTestCase> testCases = UIUtilities.CreateTestCasesForTheory(
            discoveryOptions,
            testMethod,
            theoryAttribute);
        return new(testCases);
    }
}
