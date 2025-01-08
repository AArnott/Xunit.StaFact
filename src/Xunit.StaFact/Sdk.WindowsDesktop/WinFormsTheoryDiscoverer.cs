// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WinFormsTheoryAttribute"/>.
/// </summary>
public class WinFormsTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        IXunitTestCase testCase = WinFormsUtilities.CreateTestCase(
            TestCaseKind.DataRow,
            discoveryOptions,
            testMethod,
            theoryAttribute,
            testMethodArguments);
        return new([testCase]);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        IXunitTestCase testCase = WinFormsUtilities.CreateTestCase(
            TestCaseKind.DelayEnumerated,
            discoveryOptions,
            testMethod,
            theoryAttribute,
            null);
        return new([testCase]);
    }
}
