// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="UITheoryAttribute"/>.
/// </summary>
public class UITheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        IXunitTestCase testCase = new UITestCase(UITestCase.SyncContextType.Portable, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow, settings);
        return new([testCase]);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        IXunitTestCase testCase = new UITheoryTestCase(UITestCase.SyncContextType.Portable, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, settings);
        return new([testCase]);
    }
}
