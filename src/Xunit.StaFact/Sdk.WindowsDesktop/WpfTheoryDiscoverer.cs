// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="WpfTheoryAttribute"/>.
/// </summary>
public class WpfTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        IXunitTestCase testCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.WPF, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WPF only exists on Windows.");
        return new([testCase]);
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        IXunitTestCase testCase = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITheoryTestCase(UITestCase.SyncContextType.WPF, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "WPF only exists on Windows.");
        return new([testCase]);
    }
}
