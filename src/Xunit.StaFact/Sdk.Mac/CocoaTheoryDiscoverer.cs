// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="CocoaTheoryAttribute"/>.
/// </summary>
public class CocoaTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        return new([new UITestCase(UITestCase.SyncContextType.Cocoa, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow, settings)]);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        return new([new UITheoryTestCase(UITestCase.SyncContextType.Cocoa, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, settings)]);
    }
}
