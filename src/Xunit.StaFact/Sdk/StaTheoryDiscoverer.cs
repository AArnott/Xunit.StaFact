// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Ms-PL license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xunit.Sdk;

/// <summary>
/// The discovery class for <see cref="StaTheoryAttribute"/>.
/// </summary>
public class StaTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute, ITheoryDataRow dataRow, object?[] testMethodArguments)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITestCase(UITestCase.SyncContextType.None, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "STA threads only exist on Windows.");
    }

    /// <inheritdoc/>
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, ITheoryAttribute theoryAttribute)
    {
        if (testMethod is null)
        {
            throw new ArgumentNullException(nameof(testMethod));
        }

        UISettingsAttribute settings = UIFactDiscoverer.GetSettings(testMethod);
        yield return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new UITheoryTestCase(UITestCase.SyncContextType.None, discoveryOptions.MethodDisplayOrDefault(), TestMethodDisplayOptions.None, testMethod, settings)
            : new XunitSkippedDataRowTestCase(discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod, "STA threads only exist on Windows.");
    }
}
